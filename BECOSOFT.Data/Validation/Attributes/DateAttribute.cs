using System;
using System.ComponentModel.DataAnnotations;
using BECOSOFT.Utilities.Helpers;

namespace BECOSOFT.Data.Validation.Attributes {
    /// <summary>
    /// Validates whether the <see cref="DateTime"/> value lies in the date range 0001-01-01 through 9999-12-31
    /// </summary>
    public class DateAttribute : ValidationAttribute {
        public bool AllowNull { get; set; } = true;
        public override bool IsValid(object value) {
            var nullableDate = value as DateTime?;
            if (!AllowNull && !nullableDate.HasValue) {
                return false;
            }
            var date = value as DateTime? ?? new DateTime();
            return date >= DateTimeHelpers.SqlDateMinValue &&
                   date <= DateTimeHelpers.SqlDateMaxValue;
        }
    }
}