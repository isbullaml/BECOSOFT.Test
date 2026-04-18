using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Models;
using System;
using System.Globalization;
using System.Linq;

namespace BECOSOFT.Utilities.Extensions.Numeric {
    /// <summary>
    /// Extension on the <see cref="decimal"/>-class
    /// </summary>
    public static class DecimalExtensions {
        /// <summary>
        /// Checks if the value lies in the range defined by the min and max value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>Returns whether the provided value lies in the range defined by the min and max value.</returns>
        public static bool LiesBetween(this decimal value, decimal min, decimal max) {
            return value >= min && value <= max;
        }
        /// <summary>
        /// Truncate the decimal with a given precision
        /// </summary>
        /// <param name="value">The decimal to truncate</param>
        /// <param name="precision">The precision</param>
        /// <returns>The truncated decimal</returns>
        public static decimal TruncateDecimals(this decimal value, int precision) {
            var step = (decimal) Math.Pow(10, precision);
            var truncated = Math.Truncate(step * value);
            return truncated / step;
        }

        /// <summary>Rounds a decimal value to a specified precision. A parameter specifies how to round the value if it is midway between two other numbers.</summary>
        /// <param name="value">The decimal number to round. </param>
        /// <param name="decimals">The number of significant decimal places (precision) in the return value. </param>
        /// <param name="mode">A value that specifies how to round <paramref name="value" /> if it is midway between two other numbers.</param>
        /// <returns>The rounded decimal</returns>
        public static decimal RoundTo(this decimal value, int decimals, MidpointRounding mode = MidpointRounding.AwayFromZero) {
            return decimal.Round(value, decimals, mode);
        }

        /// <summary>
        /// Converts a decimal to a <see cref="GeographicalCoordinate"/>
        /// </summary>
        /// <param name="value">The decimal to convert</param>
        /// <returns>The coordinate</returns>
        public static GeographicalCoordinate ConvertToDegrees(this decimal value) {
            var coordinate = (double) value;
            var sec = Math.Round(coordinate * 3600.0);
            var deg = sec / 3600.0;
            sec = Math.Abs(sec % 3600);
            var min = sec / 60.0;
            sec %= 60;
            // get rid of fractional part
            return new GeographicalCoordinate(deg, Math.Floor(min), Math.Floor(sec));
        }

        /// <summary>
        /// Removes the trailing zeros from a decimal
        /// </summary>
        /// <param name="value">The decimal from which the decimals should be deleted</param>
        /// <returns>The decimal without the trailing zeros</returns>
        public static decimal RemoveTrailingZeros(this decimal value) {
            //divide by 1 with x zeros (a number higher than the original value trailing zero count) to remove the all the trailing zeros
            return value / 1.00000000000000000000000000m;
        }

        public static decimal? RemoveTrailingZeros(this decimal? value) {
            //divide by 1 with x zeros (a number higher than the original value trailing zero count) to remove the all the trailing zeros
            return value / 1.00000000000000000000000000m;
        }

        /// <summary>
        /// Returns the number of decimal places of the given <see cref="value"/>. Optionally, you can specify whether to count the significant digits or unsignificant.
        /// <list type="bullet">
        /// <item><term>includeSignificant == true</term><description>If significant digits are included, the result for this decimal number 123.000456000 would be 9.</description></item>
        /// <item><term>includeSignificant == false</term><description>If significant digits are excluded, the result for this decimal number 123.000456000 would be 6.</description></item>
        /// </list>
        /// </summary>
        /// <param name="value">The value to count the decimal digits for.</param>
        /// <param name="includeSignificant">Specify whether the significant (<see langword="true"/>) or unsignificant (<see langword="false"/>) digits should be counted.</param>
        /// <returns></returns>
        public static int GetDecimalPlaces(this decimal value, bool includeSignificant = true) {
            var strRepresentation = value.ToString(CultureInfo.InvariantCulture);
            if (!includeSignificant) {
                strRepresentation = strRepresentation.TrimEnd('0');
            }
            return strRepresentation.SkipWhile(c => c != '.').Skip(1).Count();
        }

        /// <summary>
        /// Returns the number of decimal places of the given <see cref="value"/>. Optionally, you can specify whether to count the significant digits or unsignificant.
        /// <list type="bullet">
        /// <item><term>includeSignificant == true</term><description>If significant digits are included, the result for this decimal number 123.000456000 would be 9.</description></item>
        /// <item><term>includeSignificant == false</term><description>If significant digits are excluded, the result for this decimal number 123.000456000 would be 6.</description></item>
        /// </list>
        /// </summary>
        /// <param name="value">The value to count the decimal digits for.</param>
        /// <param name="includeSignificant">Specify whether the significant (<see langword="true"/>) or unsignificant (<see langword="false"/>) digits should be counted.</param>
        /// <returns></returns>
        public static int GetDecimalPlaces(this decimal? value, bool includeSignificant = true) {
            return !value.HasValue ? 0 : GetDecimalPlaces(value.Value, includeSignificant);
        }

        /// <summary>
        /// Rounds a decimal <paramref name="value"/> up or down (<paramref name="roundUp"/>) to the nearest integral part (<paramref name="numberOfIntegralDigits"/>).
        /// <para>
        /// 345.11m.RoundToNearestIntegralValue(2, true) results in 350m. 
        /// </para>
        /// <para>
        /// 334.11m.RoundToNearestIntegralValue(2, false) results in 330m. 
        /// </para>
        /// <para>
        /// 334.11m.RoundToNearestIntegralValue(1, false) results in 334m. 
        /// </para>
        /// <para>
        /// 334.11m.RoundToNearestIntegralValue(3, false) results in 300m. 
        /// </para>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="numberOfIntegralDigits"></param>
        /// <param name="roundUp"></param>
        /// <returns></returns>
        public static decimal? RoundToNearestIntegralValue(this decimal? value, int numberOfIntegralDigits, bool roundUp) {
            return value?.RoundToNearestIntegralValue(numberOfIntegralDigits, roundUp);
        }

        /// <summary>
        /// Rounds a decimal <paramref name="value"/> up or down (<paramref name="roundUp"/>) to the nearest integral part (<paramref name="numberOfIntegralDigits"/>).
        /// <para>
        /// 345.11m.RoundToNearestIntegralValue(2, true) results in 350m. 
        /// </para>
        /// <para>
        /// 334.11m.RoundToNearestIntegralValue(2, false) results in 330m. 
        /// </para>
        /// <para>
        /// 334.11m.RoundToNearestIntegralValue(1, false) results in 334m. 
        /// </para>
        /// <para>
        /// 334.11m.RoundToNearestIntegralValue(3, false) results in 300m. 
        /// </para>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="numberOfIntegralDigits"></param>
        /// <param name="roundUp"></param>
        /// <returns></returns>
        public static decimal RoundToNearestIntegralValue(this decimal value, int numberOfIntegralDigits, bool roundUp) {
            if (value == 0) { return 0;}
            var integralPart = Math.Pow(10, numberOfIntegralDigits).To<decimal>() / 10;
            decimal divided;
            if (roundUp) {
                divided = Math.Ceiling(value / integralPart);
            } else {
                divided = Math.Floor(value / integralPart);
            }
            return divided * integralPart;
        }

        /// <summary>
        /// Returns whether the <paramref name="value"/> is negative or not.
        /// Returns <see langword="true"/> when <paramref name="value"/> is &lt; <see langword="0"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNegative(this decimal value) {
            return value < 0;
        }
    }
}
