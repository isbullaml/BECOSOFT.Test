using BECOSOFT.Utilities.Attributes;
using System;
using System.Linq;

namespace BECOSOFT.Utilities.Models.Mapping.Filters {
    public enum FilterOperator {
        [LocalizedEnum("FilterOperator_EqualTo", NameResourceType = typeof(Resources))]
        EqualTo,

        [LocalizedEnum("FilterOperator_NotEqualTo", NameResourceType = typeof(Resources))]
        NotEqualTo,

        [LocalizedEnum("FilterOperator_GreaterThan", NameResourceType = typeof(Resources))]
        GreaterThan,

        [LocalizedEnum("FilterOperator_GreaterThanOrEqualTo", NameResourceType = typeof(Resources))]
        GreaterThanOrEqualTo,

        [LocalizedEnum("FilterOperator_LessThan", NameResourceType = typeof(Resources))]
        LessThan,

        [LocalizedEnum("FilterOperator_LessThanOrEqualTo", NameResourceType = typeof(Resources))]
        LessThanOrEqualTo,

        [LocalizedEnum("FilterOperator_Contains", NameResourceType = typeof(Resources))]
        Contains,

        [LocalizedEnum("FilterOperator_NotContains", NameResourceType = typeof(Resources))]
        NotContains,

        [LocalizedEnum("FilterOperator_StartsWith", NameResourceType = typeof(Resources))]
        StartsWith,

        [LocalizedEnum("FilterOperator_EndsWith", NameResourceType = typeof(Resources))]
        EndsWith,

        [LocalizedEnum("FilterOperator_LengthEqualTo", NameResourceType = typeof(Resources))]
        LengthEqualTo,

        [LocalizedEnum("FilterOperator_LengthNotEqualTo", NameResourceType = typeof(Resources))]
        LengthNotEqualTo,

        [LocalizedEnum("FilterOperator_LengthGreaterThan", NameResourceType = typeof(Resources))]
        LengthGreaterThan,

        [LocalizedEnum("FilterOperator_LengthGreaterThanOrEqualTo", NameResourceType = typeof(Resources))]
        LengthGreaterThanOrEqualTo,

        [LocalizedEnum("FilterOperator_LengthLessThan", NameResourceType = typeof(Resources))]
        LengthLessThan,

        [LocalizedEnum("FilterOperator_LengthLessThanOrEqualTo", NameResourceType = typeof(Resources))]
        LengthLessThanOrEqualTo,

        [LocalizedEnum("FilterOperator_In", NameResourceType = typeof(Resources))]
        In,

        [LocalizedEnum("FilterOperator_NotIn", NameResourceType = typeof(Resources))]
        NotIn,

        [LocalizedEnum("FilterOperator_Is", NameResourceType = typeof(Resources))]
        Is,
    }

    public static class FilterOperatorExtensions {
        public static bool IsLengthOperator(this FilterOperator filterOperator) {
            return new[] {
                FilterOperator.LengthEqualTo, FilterOperator.LengthGreaterThan, FilterOperator.LengthGreaterThanOrEqualTo,
                FilterOperator.LengthLessThan, FilterOperator.LengthLessThanOrEqualTo,
            }.Contains(filterOperator);
        }
        public static string GetOperator(this FilterOperator filterOperator) {
            switch (filterOperator) {
                case FilterOperator.EqualTo:
                case FilterOperator.LengthEqualTo:
                case FilterOperator.Is:
                    return "=";
                case FilterOperator.NotEqualTo:
                case FilterOperator.LengthNotEqualTo:
                    return "<>";
                case FilterOperator.GreaterThan:
                case FilterOperator.LengthGreaterThan:
                    return ">";
                case FilterOperator.GreaterThanOrEqualTo:
                case FilterOperator.LengthGreaterThanOrEqualTo:
                    return ">=";
                case FilterOperator.LessThan:
                case FilterOperator.LengthLessThan:
                    return "<";
                case FilterOperator.LessThanOrEqualTo:
                case FilterOperator.LengthLessThanOrEqualTo:
                    return "<=";
                case FilterOperator.Contains:
                case FilterOperator.EndsWith:
                case FilterOperator.StartsWith:
                    return "LIKE";
                case FilterOperator.NotContains:
                    return "NOT LIKE";
                case FilterOperator.In:
                    return "IN";
                case FilterOperator.NotIn:
                    return "NOT IN";
                default:
                    throw new ArgumentOutOfRangeException(nameof(filterOperator), filterOperator, null);
            }
        }

        public static bool IsNegating(this FilterOperator filterOperator) {
            switch (filterOperator) {
                case FilterOperator.NotEqualTo:
                case FilterOperator.NotContains:
                case FilterOperator.LengthNotEqualTo:
                case FilterOperator.NotIn:
                    return true;
            }
            return false;
        }

        public static bool IsEqualToOperator(this FilterOperator filterOperator) {
            switch (filterOperator) {
                case FilterOperator.EqualTo:
                case FilterOperator.NotEqualTo:
                case FilterOperator.GreaterThanOrEqualTo:
                case FilterOperator.LessThanOrEqualTo:
                case FilterOperator.LengthEqualTo:
                case FilterOperator.LengthNotEqualTo:
                case FilterOperator.LengthGreaterThanOrEqualTo:
                case FilterOperator.LengthLessThanOrEqualTo:
                    return true;
                default:
                    return false;
            }
        }
    }
}