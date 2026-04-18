using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;

namespace BECOSOFT.Utilities.Models.Mapping.Filters {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FilterPropertyCondition : FilterCondition {
        public string Entity { get; set; }
        public int? PreselectedValue { get; set; }
        public string Property { get; set; }
        public string Operator { get; set; }

        [JsonIgnore]
        public FilterOperator OperatorValue {
            get => Operator.To<FilterOperator>();
            set => Operator = value.ToString();
        }
        public object Value { get; set; }
        public List<FilterValue> Values { get; set; }

        private string DebuggerDisplay => $"{Entity}{(PreselectedValue.HasValue ? $"({PreselectedValue})" : "")}.{Property} {Operator} {Value ?? (Values.HasAny() ? string.Join(", ", Values) : "")}";
        public override bool IsValid() {
            return Value != null || Values.HasAny();
        }
    }
}