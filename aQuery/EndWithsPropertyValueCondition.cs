using System.Windows.Automation;

namespace aQuery
{
    public class EndWithsPropertyValueCondition : CustomPropertyCondition
    {
        public EndWithsPropertyValueCondition(AutomationProperty property, string value) : base(property, value)
        {
        }

        public override bool IsMatch(AutomationElement element)
        {
            return GetElementValue(element).EndsWith(Value);
        }
    }
}