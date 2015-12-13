using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Automation;
using static aQuery.Log.LogHelpers;

namespace aQuery
{
    public class SelectorItem
    {
        public static string ChildrenSeparator = " > ";
        public static string DescendantsSeparator = "\n";
        public static Dictionary<string, TreeScope> DelimiterMapping = new Dictionary<string, TreeScope>
        {
            {ChildrenSeparator, TreeScope.Children},
            {DescendantsSeparator, TreeScope.Descendants}
        };
        public static Regex SelectorPattern = new Regex(@"^('([^']+)')?(( |^)([^\[\:]+))?(\[[a-z_]+[\*\^\$]?=[^\]]*\])*(\:[a-z]+(\([^\)]+\))?)*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex PropertySelectorPattern = new Regex(@"\[([a-z_]+)([\*\^\$])?=([^\]]*)\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static Regex FilterPattern = new Regex(@"\:([a-z]+)(\(([^\)]+)\))?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public string Selector { get; set; }
        public TreeScope Scope { get; set; }

        private ConditionModel _selectorCondition;
        public ConditionModel SelectorCondition => _selectorCondition ?? (_selectorCondition = GetCondition(this));

        public static Condition CreatePropertyCondition(AutomationProperty prop, object value)
        {
            if (!(value is string))
            {
                return new PropertyCondition(prop, value);
            }

            var stringValue = (string) value;
            if (!string.IsNullOrEmpty(stringValue))
            {
                return new PropertyCondition(prop, stringValue);
            }

            return new OrCondition(
                new PropertyCondition(prop, null),
                new PropertyCondition(prop, string.Empty),
                new PropertyCondition(prop, " ")
            );
        }

        public static ConditionModel GetCondition(SelectorItem item)
        {
            var match = SelectorPattern.Match(item.Selector);
            var result = new ConditionModel();

            if (!match.Success)
            {
                result.NativeCondition = Condition.TrueCondition;
                return result;
            }

            var nativeConditions = new List<Condition>();
            var customConditions = new List<ICustomCondition>();
            var customFilters = new List<ICustomFilter>();

            if (match.Groups[1].Success)
            {
                nativeConditions.Add(CreatePropertyCondition(AutomationElement.NameProperty, match.Groups[2].Value));
            }
            if (match.Groups[3].Success)
            {
                nativeConditions.Add(CreatePropertyCondition(AutomationElement.LocalizedControlTypeProperty, match.Groups[5].Value));
            }

            var propGroup = match.Groups[6];
            if (propGroup.Success)
            {
                foreach (Capture prop in propGroup.Captures)
                {
                    var propertyMatch = PropertySelectorPattern.Match(prop.Value);

                    if (!propertyMatch.Success) continue;
                    var propertyName = propertyMatch.Groups[1].Value;
                    var propertyOperation = propertyMatch.Groups[2];
                    var propertyValue = propertyMatch.Groups[3].Success ? propertyMatch.Groups[3].Value : string.Empty;
                    var fieldInfo = typeof(AutomationElement).GetField(propertyName + "Property", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

                    if (fieldInfo == null)
                    {
                        Warn($"Property Name: `{propertyName}` doesn't found.");
                        continue;
                    }

                    var infoType = typeof(AutomationElement.AutomationElementInformation);
                    var propertyInfoType = infoType.GetProperty(propertyName);
                    var automationProperty = (AutomationProperty) fieldInfo.GetValue(null);
                    propertyValue = !string.IsNullOrEmpty(propertyValue) ? propertyValue : string.Empty;

                    if (!propertyOperation.Success)
                    {
                        var rawPropertyValue = Convert.ChangeType(propertyValue, propertyInfoType.PropertyType);
                        nativeConditions.Add(CreatePropertyCondition(automationProperty, rawPropertyValue));
                    }
                    else
                    {
                        var condition = CustomConditionHelpers.CreateCondition(automationProperty, propertyValue, propertyOperation.Value);
                        if(condition != null) customConditions.Add(condition);
                    }
                }
            }

            var filterGroup = match.Groups[7];
            if (filterGroup.Success)
            {
                foreach (Capture filter in filterGroup.Captures)
                {
                    var filterMatch = FilterPattern.Match(filter.Value);

                    if (!filterMatch.Success) continue;

                    var filterCondition = FilterHelpers.CreateFilter(filterMatch.Groups[1].Value, filterMatch.Groups[3].Success ? filterMatch.Groups[3].Value : null);
                    if (filterCondition != null) customFilters.Add(filterCondition);
                }
            }

            result.NativeCondition = nativeConditions.Count > 1 ? nativeConditions.Aggregate((x, y) => new AndCondition(x, y)) : nativeConditions[0];
            result.CustomConditions = customConditions;
            result.CustomFilters = customFilters;

            return result;
        }

        public static List<SelectorItem> SplitSelector(string selector)
        {
            var regex = new Regex("(" + DelimiterMapping.Keys.Aggregate(String.Empty, (a, b) => a + (a.Length > 0 ? "|" : String.Empty) + Regex.Escape(b)) + ")");
            var result = new List<SelectorItem>();
            string lastDelimiter = null;
            var tokens = regex.Split(selector);

            foreach (var item in tokens)
            {
                if (DelimiterMapping.ContainsKey(item))
                {
                    lastDelimiter = item;
                    continue;
                }

                result.Add(new SelectorItem
                {
                    Scope = lastDelimiter != null ? DelimiterMapping[lastDelimiter] : TreeScope.Children,
                    Selector = item
                });
                lastDelimiter = null;
            }

            return result;
        }
    }
}