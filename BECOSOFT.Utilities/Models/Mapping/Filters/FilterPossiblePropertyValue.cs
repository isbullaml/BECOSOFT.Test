using System.Diagnostics;

namespace BECOSOFT.Utilities.Models.Mapping.Filters {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FilterPossiblePropertyValue {
        public int Id { get; set; }
        public string Value { get; set; }
        public FilterPropertyValue Linked { get; set; }
        private string DebuggerDisplay => $"{Id} - {Value}, {(Linked != null ? $"Has link to: {Linked.Property}" : "")}";
    }
}