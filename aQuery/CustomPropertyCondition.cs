using System.Windows.Automation;

namespace aQuery
{
    public abstract class CustomPropertyCondition : ICustomCondition
    {
        protected CustomPropertyCondition(AutomationProperty property, string value)
        {
            Property = property;
            Value = value;
        }

        public string Value { get; set; }
        public AutomationProperty Property { get; set; }

        public string GetElementValue(AutomationElement element)
        {
            return element.GetCurrentPropertyValue(Property).ToString();
        }

        public abstract bool IsMatch(AutomationElement element);
    }
}