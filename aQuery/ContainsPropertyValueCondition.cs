using System.Windows.Automation;

namespace aQuery
{
    public class ContainsPropertyValueCondition : CustomPropertyCondition
    {
        public ContainsPropertyValueCondition(AutomationProperty property, string value) : base(property, value)
        {
        }

        public override bool IsMatch(AutomationElement element)
        {
            return GetElementValue(element).Contains(Value);
        }
    }
}