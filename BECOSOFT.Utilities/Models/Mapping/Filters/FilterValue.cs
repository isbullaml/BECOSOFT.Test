using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Helpers;
using BECOSOFT.Utilities.Models.Global;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BECOSOFT.Utilities.Models.Mapping.Filters {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FilterValue {
        public string Property { get; set; }
        public object Value { get; set; }
        public List<object> Values { get; set; }

        private string DebuggerDisplay => ToString();

        public override string ToString() {
            return $"'{Property}' - {Value ?? (Values.HasAny() ? string.Join(", ", Values) : "")}";
        }
    }

    public class FilterDateValue {
        public bool IsRelative { get; set; }
        public DateTime Value { get; set; } = DateTimeHelpers.BaseDate;

        [JsonIgnore]
        public RelativeDateConstant RelativeDateConstant { get; set; }

        public int RelativeDateConstantValue {
            get => RelativeDateConstant.To<int>();
            set => RelativeDateConstant = value.To<RelativeDateConstant>();
        }

        [JsonIgnore]
        public OffsetType OffsetType { get; set; }

        public int OffsetTypeValue {
            get => OffsetType.To<int>();
            set => OffsetType = value.To<OffsetType>();
        }

        public int OffsetValue { get; set; }

        public DateTime? GetDateTime() {
            if (!IsRelative) {
                return Value;
            }
            try {
                DateTime relativeDate;
                switch (RelativeDateConstant) {
                    case RelativeDateConstant.Now:
                        relativeDate = DateTime.Now;
                        break;
                    case RelativeDateConstant.StartOfYear:
                        relativeDate = new DateTime(DateTime.Today.Year, 1, 1);
                        break;
                    case RelativeDateConstant.StartOfMonth:
                        relativeDate = DateTime.Today.ToStartOfMonth();
                        break;
                    case RelativeDateConstant.EndOfMonth:
                        relativeDate = DateTime.Today.ToEndOfMonth();
                        break;
                    case RelativeDateConstant.EndOfYear:
                        relativeDate = new DateTime(DateTime.Today.Year, 12, 31);
                        break;
                    case RelativeDateConstant.Today:
                    case RelativeDateConstant.Unknown:
                    default:
                        relativeDate = DateTime.Today;
                        break;
                }
                switch (OffsetType) {
                    case OffsetType.None:
                        break;
                    case OffsetType.Year:
                        relativeDate = relativeDate.AddYears(OffsetValue);
                        break;
                    case OffsetType.Month:
                        relativeDate = relativeDate.AddMonths(OffsetValue);
                        break;
                    case OffsetType.Day:
                    default:
                        relativeDate = relativeDate.AddDays(OffsetValue);
                        break;
                }
                return relativeDate;
            } catch (Exception) {
                return null;
            }
        }
    }
}