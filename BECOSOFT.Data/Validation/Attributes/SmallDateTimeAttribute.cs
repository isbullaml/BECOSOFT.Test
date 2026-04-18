using System;
using System.ComponentModel.DataAnnotations;
using BECOSOFT.Utilities.Helpers;

namespace BECOSOFT.Data.Validation.Attributes {
    /// <summary>
    /// Validates whether the <see cref="DateTime"/> value lies in the smalldatetime range 1900-01-01 through 2079-06-06
    /// </summary>
    public class SmallDateTimeAttribute : ValidationAttribute {
        public bool AllowNull { get; set; } = true;
        public override bool IsValid(object value) {
            var nullableDate = value as DateTime?;
            if (AllowNull && !nullableDate.HasValue) {
                return true;
            }
            var date = nullableDate ?? new DateTime();
            return date >= DateTimeHelpers.SqlSmallDateTimeMinValue &&
                   date <= DateTimeHelpers.SqlSmallDateTimeMaxValue;
        }
    }
}