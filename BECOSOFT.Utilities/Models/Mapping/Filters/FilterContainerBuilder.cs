using BECOSOFT.Utilities.Comparers;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Models.Mapping.Filters {
    public abstract class FilterContainerBuilder {
        protected FilterContainer CreateContainer<T>(int languageID, bool skipPossibleValues) {
            var parameters = new FilterContainerParameters<T> {
                LanguageID = languageID,
                SkipPossibleValues = skipPossibleValues,
            };
            return CreateContainer(parameters);
        }

        protected FilterContainer CreateContainer<T>(FilterContainerParameters<T> parameters) {
            var container = new FilterContainer(typeof(T)) {
                LanguageID = parameters.LanguageID,
                Current = new FilterGroupingCondition {
                    LogicalGroupingValue = FilterLogicalGrouping.And,
                    Conditions = new List<FilterCondition>(0),
                }
            };
            var aliasMapping = GetAliasMapping();
            container.AliasMapping.AddRange(aliasMapping);
            var typeOperators = GetTypeOperators();
            container.TypeOperators = typeOperators;

            FillContainer(parameters, container);

            if (container.PreselectionFilterOption != null) {
                container.PreselectionCondition = new FilterGroupingCondition { LogicalGroupingValue = FilterLogicalGrouping.And };
            }

            if (parameters.SkipPossibleValues) {
                return container;
            }
            // filter empty selects
            var ascendingSortOrder = AlphanumericComparer.Ascending();
            foreach (var option in container.FilterOptions) {
                var filtered = option.Properties.Where(p => (p.DataTypeValue != FilterDataType.Select && p.DataTypeValue != FilterDataType.LinkedSelect)
                                                            || p.PreselectionFilterProperty != null || p.PossibleValues.HasAny())
                                             .OrderBy(p => p.FriendlyName, ascendingSortOrder).ToList();
                option.Properties = filtered;
                if (option.PreselectionValues.HasAny()) {
                    option.PreselectionValues.Sort((a, b) => AlphanumericComparer.CompareAscending(a.Value, b.Value));
                }
            }

            container.FilterOptions.RemoveAll(f => f.Properties.IsEmpty());
            return container;
        }

        protected abstract void FillContainer<T>(FilterContainerParameters<T> parameters, FilterContainer container);

        protected virtual List<FilterDataTypeOperator> GetTypeOperators() => DefaultTypeOperators;

        protected List<FilterDataTypeOperator> DefaultTypeOperators => new List<FilterDataTypeOperator> {
            FilterDataTypeOperator.DefaultFor(FilterDataType.Boolean),
            FilterDataTypeOperator.DefaultFor(FilterDataType.Date),
            FilterDataTypeOperator.DefaultFor(FilterDataType.Decimal),
            FilterDataTypeOperator.DefaultFor(FilterDataType.Integer),
            FilterDataTypeOperator.DefaultFor(FilterDataType.Select),
            FilterDataTypeOperator.DefaultFor(FilterDataType.String),
            FilterDataTypeOperator.DefaultFor(FilterDataType.Search),
            FilterDataTypeOperator.DefaultFor(FilterDataType.Exists),
            FilterDataTypeOperator.DefaultFor(FilterDataType.StringStrippedFromHtml),
            FilterDataTypeOperator.DefaultFor(FilterDataType.StringStrippedFromHtmlLength),
        };

        protected abstract Dictionary<Type, string> GetAliasMapping();

        protected static List<FilterBasePossibleValue> GetValues<T>(List<T> items, Func<T, int> idFunc, Func<T, string> valueFunc, bool sortByValue, string zeroValue = null) {
            if (items.IsEmpty()) {
                return new List<FilterBasePossibleValue>(0);
            }
            var values = new List<FilterPossibleValue>(items.Count);
            foreach (var item in items) {
                values.Add(new FilterPossibleValue(idFunc(item), valueFunc(item)));
            }
            if (sortByValue) {
                values = values.OrderBy(pv => pv.Value).ToList();
            }
            var result = values.Cast<FilterBasePossibleValue>().ToList();
            if (zeroValue.HasValue()) {
                result.Insert(0, new FilterPossibleValue(0, zeroValue));
            }
            return result;
        }
    }
}
