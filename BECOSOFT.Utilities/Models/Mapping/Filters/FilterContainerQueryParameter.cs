using BECOSOFT.Utilities.Extensions.Collections;
using System.Collections.Generic;

namespace BECOSOFT.Utilities.Models.Mapping.Filters {
    public class FilterContainerQueryParameter {
        public FilterContainer Container { get; set; }
        public FilterCondition PreselectionCondition { get; set; }
        public FilterCondition RootCondition { get; set; }
        public string MainTempTableName { get; set; }
        public string ParameterPrefix { get; set; }
        public FilterQueryOptions QueryOptions { get; set; } = new FilterQueryOptions();

        public FilterContainerParseParameter ToParseParameter(bool preselection) {
            var container = Container;
            if (!preselection) {
                return new FilterContainerParseParameter {
                    Container = container,
                    HasPreselectionTablePart = HasPreselectionTablePart,
                    RootCondition = RootCondition,
                    MainTempTableName = MainTempTableName,
                    ParameterPrefix = ParameterPrefix,
                    QueryOptions = QueryOptions,
                };
            }
            var mainType = container.PreselectionFilterOption.EntityType;
            var result = new FilterContainerParseParameter {
                HasPreselectionTablePart = false,
                RootCondition = PreselectionCondition,
                Container = new FilterContainer(mainType) {
                    FilterOptions = new List<FilterOption> {
                        container.PreselectionFilterOption,
                    },
                    Current = container.PreselectionCondition,
                    TypeOperators = container.TypeOperators,
                },
                MainTempTableName = MainTempTableName,
                ParameterPrefix = ParameterPrefix,
                IsPreselection = true,
                QueryOptions = QueryOptions,
            };
            result.Container.AliasMapping.AddRange(container.AliasMapping);
            return result;
        }

        public bool HasPreselectionTablePart {
            get {
                if (Container.PreselectionFilterOption == null) { return false; }
                if (PreselectionCondition == null) { return false; }
                return PreselectionCondition.IsValid();
            }
        }

        public bool IsValid() {
            if (Container == null) {
                return false;
            }
            if (RootCondition != null && !RootCondition.IsValid()) {
                return false;
            }
            return true;
        }
    }

    public class FilterQueryOptions {
        public string TempTable { get; set; }
        public string JoinPart { get; set; }
        public string PreselectionJoinPart { get; set; }
    }

    public class FilterContainerParseParameter {
        public bool IsPreselection { get; set; }
        public bool HasPreselectionTablePart { get; set; }
        public FilterCondition RootCondition { get; set; }
        public FilterContainer Container { get; set; }
        public string MainTempTableName { get; set; }
        public string ParameterPrefix { get; set; }
        public FilterQueryOptions QueryOptions { get; set; }
    }
}
