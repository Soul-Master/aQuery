using System.Windows.Automation;

namespace aQuery
{
    public interface ICustomCondition
    {
        bool IsMatch(AutomationElement element);
    }
}