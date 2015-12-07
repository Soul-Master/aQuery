using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Automation;

namespace aQuery
{
    public class FirstFilter : ICustomFilter
    {
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public IEnumerable<AutomationElement> Filter(IEnumerable<AutomationElement> elements)
        {
            return new List<AutomationElement> { elements.First() };
        }
    }
}