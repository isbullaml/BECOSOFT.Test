using BECOSOFT.Utilities.Helpers;
using System.Diagnostics;

namespace BECOSOFT.Utilities.Models.Mapping.Filters {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FilterOperatorValue {
        public string Code { get; set; }
        public string Value { get; set; }

        public FilterOperatorValue(FilterOperator op) {
            Code = op.ToString().ToLower();
            Value = op.GetLocalizedDescription();
        }

        private string DebuggerDisplay => $"{Code} - {Value}";
    }
}