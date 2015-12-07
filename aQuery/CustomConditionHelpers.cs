using System.Windows.Automation;

namespace aQuery
{
    public static class CustomConditionHelpers
    {
        public static ICustomCondition CreateCondition(AutomationProperty property, string value, string operation)
        {
            switch (operation)
            {
                case "*": return new ContainsPropertyValueCondition(property, value);
                case "^": return new StartWithsPropertyValueCondition(property, value);
                case "$": return new EndWithsPropertyValueCondition(property, value);
            }

            return null;
        }
    }
}