using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;

namespace BECOSOFT.Utilities.Models.Mapping.Filters {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FilterGroupingCondition : FilterCondition {
        [JsonIgnore]
        public FilterLogicalGrouping LogicalGroupingValue { get; set; }

        public string LogicalGrouping {
            get => LogicalGroupingValue.ToString().ToLower();
            set => LogicalGroupingValue = value.To<FilterLogicalGrouping>();
        }

        public List<FilterCondition> Conditions { get; set; }

        private string DebuggerDisplay => $"{LogicalGrouping}, {Conditions?.Count} conditions";

        public override bool IsValid() {
            return Conditions.HasAny();
        }
    }
}