using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Automation;

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
        public static Regex SelectorPattern = new Regex(@"^('([^']+)')?(( |^)([^\[]+))?(\[[a-z_]+=[^\]]*\])*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex PropertySelectorPattern = new Regex(@"\[([a-z_]+)=([^\]]*)\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public string Selector { get; set; }
        public TreeScope Scope { get; set; }

        public Condition GetCondition()
        {
            var match = SelectorPattern.Match(Selector);
            if (!match.Success) return Condition.TrueCondition;

            var conditions = new List<Condition>();
            if (match.Groups[1].Success)
            {
                conditions.Add(new PropertyCondition(AutomationElement.NameProperty, match.Groups[2].Value));
            }
            if (match.Groups[3].Success)
            {
                conditions.Add(new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, match.Groups[5].Value));
            }

            var propGroup = match.Groups[6];
            if (propGroup.Success)
            {
                foreach (Capture prop in propGroup.Captures)
                {
                    var propertyMatch = PropertySelectorPattern.Match(prop.Value);

                    if (!propertyMatch.Success) continue;
                    var propertyName = propertyMatch.Groups[1].Value;
                    var propertyValue = propertyMatch.Groups[2].Success ? propertyMatch.Groups[2].Value : string.Empty;
                    var fieldInfo = typeof(AutomationElement).GetField(propertyName + "Property", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

                    if (fieldInfo == null)
                    {
                        Console.WriteLine($"Property Name: `{propertyName}` doesn't found.");
                        continue;
                    }

                    var infoType = typeof(AutomationElement.AutomationElementInformation);
                    var propertyInfoType = infoType.GetProperty(propertyName);

                    var automationProperty = (AutomationProperty) fieldInfo.GetValue(null);
                    var rawPropertyValue = Convert.ChangeType(propertyValue, propertyInfoType.PropertyType);
                    conditions.Add(new PropertyCondition(automationProperty, rawPropertyValue));
                }
            }

            return conditions.Count > 1 ? conditions.Aggregate((x, y) => new AndCondition(x, y)) : conditions[0];
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