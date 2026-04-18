using System.Collections.Generic;
using System.Diagnostics;

namespace BECOSOFT.Utilities.Models.Mapping.Filters {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FilterPropertyValue : FilterBasePossibleValue {
        public string Property { get; set; }
        public List<FilterPossiblePropertyValue> Values { get; set; }
        private string DebuggerDisplay => $"{Property}, {Values.Count} values";
    }
}