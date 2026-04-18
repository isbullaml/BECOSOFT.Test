using System.Diagnostics;

namespace BECOSOFT.Utilities.Models.Mapping.Filters {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FilterDataTypeValue {
        public string Value { get; set; }
        public string FriendlyName { get; set; }

        public FilterDataTypeValue(string value, string friendlyName) {
            Value = value;
            FriendlyName = friendlyName;
        }

        private string DebuggerDisplay => $"{Value} ({FriendlyName})";
    }
}