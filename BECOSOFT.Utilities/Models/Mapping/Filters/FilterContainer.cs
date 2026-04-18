using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BECOSOFT.Utilities.Models.Mapping.Filters {
    public class FilterContainer {
        [JsonIgnore]
        public Type MainType { get; }

        [JsonIgnore]
        public readonly Dictionary<Type, string> AliasMapping;

        [JsonIgnore]
        public int LanguageID { get; set; }

        [JsonIgnore]
        public List<string> ExtraResultColumns { get; set; }

        public FilterCondition PreselectionCondition { get; set; }
        public FilterCondition Current { get; set; }

        public List<FilterOption> FilterOptions { get; set; }
        public FilterOption PreselectionFilterOption { get; set; }

        public List<FilterDataTypeOperator> TypeOperators { get; set; }

        public FilterContainer(Type mainType) {
            MainType = mainType;
            AliasMapping = new Dictionary<Type, string>();
            FilterOptions = new List<FilterOption>();
            PreselectionFilterOption = null;
        }

        public FilterContainer Copy() {
            return new FilterContainer(MainType) {
                Current = null,
                PreselectionCondition = null,
                FilterOptions = FilterOptions,
                LanguageID = LanguageID,
                PreselectionFilterOption = PreselectionFilterOption,
                TypeOperators = TypeOperators,
            };
        }
    }

    public class FilterConditionResult {
        public StringBuilder QueryPart { get; set; }
        public Tuple<Type, string, string> UsedTableAlias { get; set; }
        public readonly List<Tuple<Type, string, string>> TableAliases = new List<Tuple<Type, string, string>>(0);
    }
}