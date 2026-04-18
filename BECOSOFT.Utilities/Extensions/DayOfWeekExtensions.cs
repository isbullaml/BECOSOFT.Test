using BECOSOFT.Utilities.Models;
using System;

namespace BECOSOFT.Utilities.Extensions {
    /// <summary>
    /// Extensions for converting the day of the week between systems
    /// </summary>
    public static class DayOfWeekExtensions {
        /// <summary>
        /// Converts a SQL-day to a .NET-day
        /// </summary>
        /// <param name="sqlDayOfWeek">The SQL-day</param>
        /// <returns>The .NET-date</returns>
        public static DayOfWeek Convert(this SqlDayOfWeek sqlDayOfWeek) {
            var dayOfWeek = DayOfWeek.Sunday;
            switch (sqlDayOfWeek) {
                case SqlDayOfWeek.Sunday:
                    dayOfWeek = DayOfWeek.Sunday;
                    break;
                case SqlDayOfWeek.Monday:
                    dayOfWeek = DayOfWeek.Monday;
                    break;
                case SqlDayOfWeek.Tuesday:
                    dayOfWeek = DayOfWeek.Tuesday;
                    break;
                case SqlDayOfWeek.Wednesday:
                    dayOfWeek = DayOfWeek.Wednesday;
                    break;
                case SqlDayOfWeek.Thursday:
                    dayOfWeek = DayOfWeek.Thursday;
                    break;
                case SqlDayOfWeek.Friday:
                    dayOfWeek = DayOfWeek.Friday;
                    break;
                case SqlDayOfWeek.Saturday:
                    dayOfWeek = DayOfWeek.Saturday;
                    break;
            }
            return dayOfWeek;
        }

        /// <summary>
        /// Converts a .NET-day to it's corresponding short-string
        /// </summary>
        /// <param name="dayOfWeek">The .NET-day</param>
        /// <returns>The short-string</returns>
        public static string ToShortString(this DayOfWeek dayOfWeek) {
            var result = "";
            switch (dayOfWeek) {
                case DayOfWeek.Sunday:
                    result = Resources.DayOfWeek_Sunday_Short;
                    break;
                case DayOfWeek.Monday:
                    result = Resources.DayOfWeek_Monday_Short;
                    break;
                case DayOfWeek.Tuesday:
                    result = Resources.DayOfWeek_Tuesday_Short;
                    break;
                case DayOfWeek.Wednesday:
                    result = Resources.DayOfWeek_Wednesday_Short;
                    break;
                case DayOfWeek.Thursday:
                    result = Resources.DayOfWeek_Thursday_Short;
                    break;
                case DayOfWeek.Friday:
                    result = Resources.DayOfWeek_Friday_Short;
                    break;
                case DayOfWeek.Saturday:
                    result = Resources.DayOfWeek_Saturday_Short;
                    break;
            }
            return result;
        }

        /// <summary>
        /// Converts a .NET-day to it's corresponding long-string
        /// </summary>
        /// <param name="dayOfWeek">The .NET-day</param>
        /// <returns>The long-string</returns>
        public static string ToLongString(this DayOfWeek dayOfWeek) {
            var result = "";
            switch (dayOfWeek) {
                case DayOfWeek.Sunday:
                    result = Resources.DayOfWeek_Sunday;
                    break;
                case DayOfWeek.Monday:
                    result = Resources.DayOfWeek_Monday;
                    break;
                case DayOfWeek.Tuesday:
                    result = Resources.DayOfWeek_Tuesday;
                    break;
                case DayOfWeek.Wednesday:
                    result = Resources.DayOfWeek_Wednesday;
                    break;
                case DayOfWeek.Thursday:
                    result = Resources.DayOfWeek_Thursday;
                    break;
                case DayOfWeek.Friday:
                    result = Resources.DayOfWeek_Friday;
                    break;
                case DayOfWeek.Saturday:
                    result = Resources.DayOfWeek_Saturday;
                    break;
            }
            return result;
        }

        /// <summary>
        /// Converts a .NET-day to a sortable entity
        /// </summary>
        /// <param name="dayOfWeek">The .NET-day</param>
        /// <returns>The index-value of the day</returns>
        public static int ToSortable(this DayOfWeek dayOfWeek) {
            return ((int) dayOfWeek + 6) % 7;
        }
    }
}
