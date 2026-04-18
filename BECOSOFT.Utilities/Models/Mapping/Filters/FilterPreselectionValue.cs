using System.Collections.Generic;
using System.Diagnostics;

namespace BECOSOFT.Utilities.Models.Mapping.Filters {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FilterPreselectionValue {
        public int Id { get; set; }

        public string Value { get; set; }

        public List<FilterProperty> Properties { get; set; }

        public FilterPreselectionValue(int id, string value) {
            Id = id;
            Value = value;
        }

        private string DebuggerDisplay => $"{Id} - {Value}";
    }
}