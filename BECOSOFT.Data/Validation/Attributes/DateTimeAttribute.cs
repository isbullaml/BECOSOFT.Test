using System;
using System.ComponentModel.DataAnnotations;
using BECOSOFT.Utilities.Helpers;

namespace BECOSOFT.Data.Validation.Attributes {
    /// <summary>
    /// Validates whether the <see cref="DateTime"/> value lies in the datetime range 1753-01-01 through 9999-12-31
    /// </summary>
    public class DateTimeAttribute : ValidationAttribute {
        public bool AllowNull { get; set; } = true;
        public override bool IsValid(object value) {
            var nullableDate = value as DateTime?;
            if (AllowNull && !nullableDate.HasValue) {
                return true;
            }
            var date = nullableDate ?? new DateTime();
            return date >= DateTimeHelpers.SqlDateTimeMinValue &&
                   date <= DateTimeHelpers.SqlDateTimeMaxValue;
        }
    }
}