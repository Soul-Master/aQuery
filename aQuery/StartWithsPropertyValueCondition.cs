using System.Windows.Automation;

namespace aQuery
{
    public class StartWithsPropertyValueCondition : CustomPropertyCondition
    {
        public StartWithsPropertyValueCondition(AutomationProperty property, string value) : base(property, value)
        {
        }

        public override bool IsMatch(AutomationElement element)
        {
            return GetElementValue(element).StartsWith(Value);
        }
    }
}