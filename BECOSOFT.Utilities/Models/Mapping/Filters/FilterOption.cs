using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BECOSOFT.Utilities.Models.Mapping.Filters {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FilterOption {
        [JsonIgnore]
        public Type EntityType { get; set; }

        [JsonIgnore]
        public string ForeignKeyProperty { get; set; }

        [JsonIgnore]
        public List<string> ExtraForeignKeyProperties { get; set; }

        public string Entity { get; set; }

        public string FriendlyName { get; set; }

        public List<FilterPreselectionValue> PreselectionValues { get; set; }

        public List<FilterProperty> Properties { get; set; }

        [JsonIgnore]
        public string PreselectionFilterProperty { get; set; }

        [JsonIgnore]
        public string PreselectionFilterEntity { get; set; }

        [JsonIgnore]
        public string PreselectedValuePlaceholder { get; set; }

        [JsonIgnore]
        public bool HasMultipleValues { get; set; }

        [JsonIgnore]
        public Dictionary<int, string> PreselectionFilterTableNames { get; set; }

        private string DebuggerDisplay => $"{Entity} ({FriendlyName}), {Properties.Count} properties";
    }
}