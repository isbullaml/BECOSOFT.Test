using BECOSOFT.Utilities.Helpers;
using BECOSOFT.Utilities.Models;
using System;
using System.Globalization;

namespace BECOSOFT.Utilities.Extensions {
    /// <summary>
    /// Extensions on the <see cref="DateTime"/>-class
    /// </summary>
    public static class DateTimeExtensions {
        /// <summary>Gets the <see cref="Month"/>-enum component of the date represented by this instance.</summary>
        /// <param name="dateTime"><see cref="DateTime"/> to get the <see cref="Month"/>-enum value for.</param>
        /// <returns>The month component, expressed as a <see cref="Month"/>-enum value.</returns>
        public static Month GetMonth(this DateTime dateTime) {
            return (Month) dateTime.Month;
        }

        /// <summary>
        /// Converts a datetime to an SQL-date
        /// </summary>
        /// <param name="dateTime">The datetime</param>
        /// <returns>The string for SQL-usage</returns>
        public static string ToSqlDate(this DateTime dateTime) {
            return "convert(date, '" + dateTime.ToString("yyyy-MM-dd") + "') ";
        }

        /// <summary>
        /// Converts a datetime to an SQL-datetime
        /// </summary>
        /// <param name="dateTime">The datetime</param>
        /// <param name="zeroTime">Value indicating whether 00:00:00 should be used as time</param>
        /// <returns>The string for SQL-usage</returns>
        public static string ToSqlDateTime(this DateTime dateTime, bool zeroTime = false) {
            /*
            Important note:
            https://docs.microsoft.com/en-us/sql/t-sql/data-types/datetime-transact-sql
            999 gets rounded up to 000
            995-998 gets rounded to 997
            992-994 gets rounded to 993
            990-991 gets rounded to 990
             */
            var format = "yyyy-MM-dd " + (zeroTime ? "00:00:00" : "HH:mm:ss.fff");
            return "convert(datetime, '" + dateTime.ToString(format) + "', 121) ";
        }

        /// <summary>
        /// Check if the date is the base date (1900-01-01)
        /// </summary>
        /// <param name="value">The datetime</param>
        /// <returns>Value indicating whether the datetime is the base date</returns>
        public static bool IsBaseDate(this DateTime value) {
            return value == DateTimeHelpers.BaseDate;
        }

        /// <summary>
        /// Check if the date is the base date (1900-01-01)
        /// </summary>
        /// <param name="value">The datetime</param>
        /// <returns>Value indicating whether the datetime is the base date</returns>
        public static bool IsBaseDate(this DateTime? value) {
            return value.HasValue && IsBaseDate(value.Value);
        }

        /// <summary>
        /// Initializes a new instance of the DateTime structure with a supplied precision
        /// </summary>
        /// <param name="dateTime">The DateTime instance to truncate</param>
        /// <param name="precision">The precision for a truncate</param>
        /// <returns></returns>
        public static DateTime Truncate(this DateTime dateTime, DateTimePrecision precision) {
            long roundingTimeSpan;
            switch (precision) {
                case DateTimePrecision.Hour:
                    roundingTimeSpan = TimeSpan.TicksPerHour;
                    break;
                case DateTimePrecision.Minute:
                    roundingTimeSpan = TimeSpan.TicksPerMinute;
                    break;
                case DateTimePrecision.Second:
                    roundingTimeSpan = TimeSpan.TicksPerSecond;
                    break;
                default:
                    return dateTime;
            }
            return new DateTime(dateTime.Ticks - dateTime.Ticks % roundingTimeSpan);
        }

        /// <summary>
        /// Retrieves the weeknumber from a given DateTime instance.
        /// </summary>
        /// <param name="dateTime">The DateTime instance</param>
        /// <returns></returns>
        public static int WeekNumber(this DateTime dateTime) {
            // http://stackoverflow.com/questions/11154673/get-the-correct-week-number-of-a-given-date
            var day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(dateTime);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday) {
                dateTime = dateTime.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        /// <summary>
        /// Gets a <see cref="DateTime"/> containing the first day of the provided month and year (from <paramref name="dateTime"/>).
        /// </summary>
        /// <param name="dateTime">The DateTime instance</param>
        /// <returns>A <see cref="DateTime"/> containing the first day of the provided month and year (from <paramref name="dateTime"/>).</returns>
        public static DateTime ToStartOfMonth(this DateTime dateTime) {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }

        /// <summary>
        /// Gets a <see cref="DateTime"/> containing the last day of the provided month and year (from <paramref name="dateTime"/>).
        /// </summary>
        /// <param name="dateTime">The DateTime instance</param>
        /// <returns>A <see cref="DateTime"/> containing the last day of the provided month and year (from <paramref name="dateTime"/>).</returns>
        public static DateTime ToEndOfMonth(this DateTime dateTime) {
            return new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
        }

        /// <summary>
        /// Gets a <see cref="DateTime"/> containing the first day of the provided year (from <paramref name="dateTime"/>).
        /// </summary>
        /// <param name="dateTime">The DateTime instance</param>
        /// <returns>A <see cref="DateTime"/> containing the first day of the provided year (from <paramref name="dateTime"/>).</returns>
        public static DateTime ToStartOfYear(this DateTime dateTime) {
            return new DateTime(dateTime.Year, 1, 1);
        }

        /// <summary>
        /// Gets a <see cref="DateTime"/> containing the last day of the provided year (from <paramref name="dateTime"/>).
        /// </summary>
        /// <param name="dateTime">The DateTime instance</param>
        /// <returns>A <see cref="DateTime"/> containing the last day of the provided year (from <paramref name="dateTime"/>).</returns>
        public static DateTime ToEndOfYear(this DateTime dateTime) {
            return new DateTime(dateTime.Year, 12, DateTime.DaysInMonth(dateTime.Year, 12));
        }

        /// <summary>
        /// Gets a <see cref="DateTime"/> containing the first day (<see cref="DateTime.Date"/> part) of the week for the provided <see cref="dateTime"/>.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime ToStartOfWeek(this DateTime dateTime) {
            var ci = CultureInfo.CurrentCulture;
            var firstDayOfWeek = ci.DateTimeFormat.FirstDayOfWeek;
            var thisDayOfWeek = dateTime.DayOfWeek;
            var diff = (7 + (thisDayOfWeek - firstDayOfWeek)) % 7;
            return dateTime.AddDays(-1 * diff).Date;
        }

        /// <summary>
        /// Gets a <see cref="DateTime"/> containing the last day (<see cref="DateTime.Date"/> part) of the week for the provided <see cref="dateTime"/>.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime ToEndOfWeek(this DateTime dateTime) {
            return dateTime.ToStartOfWeek().AddDays(6);
        }

        /// <summary>
        /// <para>Returns the as a Julian Date string format (YYYYDDD)</para>
        /// <para>Example: 15/01/2011 will turn into 2011015</para>
        /// <para>Source: https://stackoverflow.com/questions/5805406/how-do-you-convert-a-datetime-to-juliandate</para>
        /// </summary>
        /// <param name="dateTime">The DateTime instance</param>
        /// <returns></returns>
        public static string ToJulianDatestring(this DateTime dateTime) {
            return $"{dateTime:yyyy}{dateTime.DayOfYear:D3}";
        }

        public static DateTime ToStartOfDay(this DateTime dateTime) {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0);
        }

        public static DateTime ToEndOfDay(this DateTime dateTime) {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59, 997);
        }

        public static int GetNumberOfWeeksInMonth(this DateTime date) {
            var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            var firstOfMonth = new DateTime(date.Year, date.Month, 1);
            var endOfMonth = new DateTime(date.Year, date.Month, daysInMonth);
            var cal = new GregorianCalendar();
            var weekRule = DateTimeFormatInfo.CurrentInfo.CalendarWeekRule;
            var firstDayOfWeek = DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek;
            return cal.GetWeekOfYear(endOfMonth, weekRule, firstDayOfWeek) - cal.GetWeekOfYear(firstOfMonth, weekRule, firstDayOfWeek) + 1;
        }

        public static DateTime GetLastYearDate(this DateTime date) {
            var lastYear = date.Year - 1;
            var isoWeek = date.ISOWeekNumber();
            var dayOfWeek = date.DayOfWeek;

            return ISOWeek.ToDateTime(lastYear, isoWeek, dayOfWeek);
        }

        public static int ISOWeekNumber(this DateTime dateTime) {
            var weekDay = dateTime.DayOfWeek == DayOfWeek.Sunday ? 7 : (int) dateTime.DayOfWeek;
            var weekNumber = (dateTime.DayOfYear - weekDay + 10) / 7;
            if (weekNumber < 1) {
                return GetWeeksInYear(dateTime.Year - 1);
            }
            if (weekNumber > GetWeeksInYear(dateTime.Year)) {
                return 1;
            }
            return weekNumber;
        }

        private static int GetWeeksInYear(int year) {
            int Test(int y) => (y + (y / 4) - (y / 100) + (y / 400)) % 7;

            if (Test(year) == 4 || Test(year - 1) == 3) {
                return 53;
            }
            return 52;
        }

        /// <summary>
        /// Returns the number of seconds sinds the Unix Epoch (1970-01-01)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ToUnixTime(this DateTime dateTime) {
            var timeSpan = dateTime - DateTimeHelpers.UnixEpochDate;
            return (long)timeSpan.TotalSeconds;
        }
    }
}