using System.Collections.Generic;
using System.Windows.Automation;

namespace aQuery
{
    public interface ICustomFilter
    {
        IEnumerable<AutomationElement> Filter(IEnumerable<AutomationElement> element);
    }
}