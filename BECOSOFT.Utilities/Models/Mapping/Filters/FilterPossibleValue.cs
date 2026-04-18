using System.Diagnostics;

namespace BECOSOFT.Utilities.Models.Mapping.Filters {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FilterPossibleValue : FilterBasePossibleValue {
        public int Id { get; set; }

        public string Value { get; set; }

        public FilterPossibleValue() {
        }

        public FilterPossibleValue(int id, string value) {
            Id = id;
            Value = value;
        }

        private string DebuggerDisplay => $"{Id} - {Value}";
    }
}