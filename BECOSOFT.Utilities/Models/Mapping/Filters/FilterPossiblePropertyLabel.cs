using System.Diagnostics;

namespace BECOSOFT.Utilities.Models.Mapping.Filters {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FilterPossiblePropertyLabel {
        public string Property { get; set; }
        public string FriendlyName { get; set; }
        private string DebuggerDisplay => $"{Property} ({FriendlyName})";
    }
}