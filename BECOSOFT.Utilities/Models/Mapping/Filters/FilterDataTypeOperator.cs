using BECOSOFT.Utilities.Collections;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Helpers;
using BECOSOFT.Utilities.Models.Global;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BECOSOFT.Utilities.Models.Mapping.Filters {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FilterDataTypeOperator {
        [JsonIgnore]
        public FilterDataType DataTypeValue { get; set; }

        public string DataType {
            get => DataTypeValue.ToString().ToLower();
            set => DataTypeValue = value.To<FilterDataType>();
        }

        [JsonIgnore]
        public List<FilterOperator> OperatorValues { get; set; }

        public List<FilterOperatorValue> Operators {
            get { return OperatorValues.Select(o => new FilterOperatorValue(o)).ToList(); }
            set { OperatorValues = value?.Select(v => v.To<FilterOperator>()).ToList(); }
        }

        public List<FilterDataTypeValue> DataTypeValues { get; set; }

        [JsonIgnore]
        public FilterInputType InputType { get; set; }

        public string InputTypeString {
            get => InputType.ToString().ToLower();
            set => InputType = value.To<FilterInputType>();
        }

        public KeyValueList<int, string> RelativeDateValues { get; set; }
        public KeyValueList<int, string> OffsetTypeValues { get; set; }

        private string DebuggerDisplay => $"{DataTypeValue}";

        public static FilterDataTypeOperator DefaultFor(FilterDataType dataType) {
            var result = new FilterDataTypeOperator {
                DataTypeValue = dataType,
            };
            switch (dataType) {
                case FilterDataType.Boolean:
                case FilterDataType.Exists:
                    result.InputType = FilterInputType.Radio;
                    result.OperatorValues = new List<FilterOperator> {
                        FilterOperator.Is,
                    };
                    result.DataTypeValues = new List<FilterDataTypeValue> {
                        new FilterDataTypeValue(true.ToString(), Resources.General_True),
                        new FilterDataTypeValue(false.ToString(), Resources.General_False),
                    };
                    break;
                case FilterDataType.String:
                    result.InputType = FilterInputType.Text;
                    result.OperatorValues = new List<FilterOperator> {
                        FilterOperator.EqualTo,
                        FilterOperator.NotEqualTo,
                        FilterOperator.Contains,
                        FilterOperator.NotContains,
                        FilterOperator.StartsWith,
                        FilterOperator.EndsWith,
                        FilterOperator.LengthEqualTo,
                        FilterOperator.LengthNotEqualTo,
                        FilterOperator.LengthGreaterThan,
                        FilterOperator.LengthGreaterThanOrEqualTo,
                        FilterOperator.LengthLessThan,
                        FilterOperator.LengthLessThanOrEqualTo,
                    };
                    break;
                case FilterDataType.StringStrippedFromHtml:
                    result.InputType = FilterInputType.Text;
                    result.OperatorValues = new List<FilterOperator> {
                        FilterOperator.EqualTo,
                        FilterOperator.NotEqualTo,
                        FilterOperator.Contains,
                        FilterOperator.NotContains,
                        FilterOperator.StartsWith,
                        FilterOperator.EndsWith,
                    };
                    break;
                case FilterDataType.StringStrippedFromHtmlLength:
                    result.InputType = FilterInputType.Text;
                    result.OperatorValues = new List<FilterOperator> {
                        FilterOperator.LengthEqualTo,
                        FilterOperator.LengthNotEqualTo,
                        FilterOperator.LengthGreaterThan,
                        FilterOperator.LengthGreaterThanOrEqualTo,
                        FilterOperator.LengthLessThan,
                        FilterOperator.LengthLessThanOrEqualTo,
                    };
                    break;
                case FilterDataType.Integer:
                case FilterDataType.Date:
                case FilterDataType.Decimal:
                    result.InputType = FilterInputType.Text;
                    result.OperatorValues = new List<FilterOperator> {
                        FilterOperator.EqualTo,
                        FilterOperator.NotEqualTo,
                        FilterOperator.GreaterThan,
                        FilterOperator.GreaterThanOrEqualTo,
                        FilterOperator.LessThan,
                        FilterOperator.LessThanOrEqualTo,
                    };
                    if (dataType == FilterDataType.Date) {
                        result.RelativeDateValues = EnumHelper.GetKeyValueList<int>(typeof(RelativeDateConstant));
                        result.OffsetTypeValues = EnumHelper.GetKeyValueList<int>(typeof(OffsetType));
                    }
                    break;
                case FilterDataType.Select:
                    result.InputType = FilterInputType.Select;
                    result.OperatorValues = new List<FilterOperator> {
                        FilterOperator.In,
                        FilterOperator.NotIn,
                    };
                    break;
                case FilterDataType.LinkedSelect:
                    result.InputType = FilterInputType.Select;
                    result.OperatorValues = new List<FilterOperator> {
                        FilterOperator.EqualTo,
                        FilterOperator.NotEqualTo,
                    };
                    break;
                case FilterDataType.Search:
                    result.InputType = FilterInputType.Search;
                    result.OperatorValues = new List<FilterOperator> {
                        FilterOperator.In,
                        FilterOperator.NotIn,
                    };
                    break;
            }
            return result;
        }
    }
}