using System.Collections.Generic;
using System.Windows.Automation;

namespace aQuery
{
    public class ConditionModel
    {
        public Condition NativeCondition { get; set; }
        public List<ICustomCondition> CustomConditions { get; set; }
        public List<ICustomFilter> CustomFilters { get; set; }
    }
}