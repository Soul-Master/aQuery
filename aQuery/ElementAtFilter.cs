using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Automation;

namespace aQuery
{
    public class ElementAtFilter : ICustomFilter
    {
        public ElementAtFilter(string value)
        {
            Index = int.Parse(value);
        }

        public int Index { get; }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public IEnumerable<AutomationElement> Filter(IEnumerable<AutomationElement> elements)
        {
            if (elements.Count() <= Index) return Enumerable.Empty<AutomationElement>();

            return new List<AutomationElement> {elements.ElementAt(Index)};
        }
    }
}