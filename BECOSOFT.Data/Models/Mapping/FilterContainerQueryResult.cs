using BECOSOFT.Utilities.Models.Mapping.Filters;

namespace BECOSOFT.Data.Models.Mapping {
    public class FilterContainerQueryResult {
        public bool Success { get; set; }
        public ParametrizedQuery Query { get; set; }
        public ParametrizedQuery PreselectionQuery { get; set; }
        public FilterContainerQueryParameter Parameters { get; set; }
    }
}
