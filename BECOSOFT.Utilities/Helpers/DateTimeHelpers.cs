using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

// ReSharper disable InvalidXmlDocComment

namespace BECOSOFT.Utilities.Helpers {
    /// <summary>
    /// Helper class for a <see cref="DateTime"/>
    /// </summary>
    public static class DateTimeHelpers {
        /// <summary>
        /// The base year: <see langword="1900"/>
        /// </summary>
        public const int BaseYear = 1900;

        /// <summary>
        /// The base date: <see langword="1900-01-01"/>
        /// </summary>
        public static DateTime BaseDate => new DateTime(BaseYear, (byte)Month.January, 1);

        /// <summary>
        /// The minimum value for an SQL small DateTime: <see langword="1900-01-01"/>
        /// </summary>
        public static DateTime SqlSmallDateTimeMinValue => new DateTime(BaseYear, (byte)Month.January, 1);

        /// <summary>
        /// The maximum value for an SQL small DateTime: <see langword="2079-06-06 23:59:00.000"/>
        /// </summary>
        public static DateTime SqlSmallDateTimeMaxValue => new DateTime(2079, (byte)Month.June, 06, 23, 59, 00);

        /// <summary>
        /// The minimum value for an SQL DateTime: <see langword="1753-01-01 00:00:00.000"/>
        /// </summary>
        public static DateTime SqlDateTimeMinValue => new DateTime(1753, (byte)Month.January, 1);

        /// <summary>
        /// The maximum value for an SQL DateTime: <see langword="9999-12-31 23:59:59:997"/>
        /// </summary>
        public static DateTime SqlDateTimeMaxValue => new DateTime(9999, (byte)Month.December, 31, 23, 59, 59, 997);

        /// <summary>
        /// The minimum value for an SQL DateTime: <see langword="0001-01-01"/>
        /// </summary>
        public static DateTime SqlDateMinValue => new DateTime(1, (byte)Month.January, 1);

        /// <summary>
        /// The maximum value for an SQL DateTime: <see langword="9999-12-31 23:59:59:997"/>
        /// </summary>
        public static DateTime SqlDateMaxValue => new DateTime(9999, (byte)Month.December, 31, 23, 59, 59, 997);

        /// <summary>
        /// Starting point for Unix time calculation
        /// </summary>
        // Link: https://en.wikipedia.org/wiki/Unix_time
        public static DateTime UnixEpochDate => new DateTime(1970, 1, 1);

        /// <summary>
        /// The date representing an invalid date in Excel
        /// </summary>
        //reason for the invalidExcelDateCheck:
        // https: //learn.microsoft.com/nl-NL/office/troubleshoot/excel/wrongly-assumes-1900-is-leap-year
        // https: //social.msdn.microsoft.com/Forums/sqlserver/en-US/8660f0e2-7a0a-4a96-a958-6514d867e1fe/18991230-000000000-appears-in-date-time-type-columns?forum=sqlintegrationservices
        // https: //stackoverflow.com/questions/62182077/ssis-package-inserting-12-31-1899-instead-of-1-1-1900-that-was-provided
        // Excel subtracts a day on dates that are smaller than 1900-03-01 (because it thinks 1900-02-29 existed that year - which it didn't).
        public static DateTime InvalidExcelDate => BaseDate.AddDays(-1);

        public static short MaximumDaysInYear => 366;
        public static byte MaximumWeeksInYear => 53;
        public static byte MaximumDaysInMonth => 31;
        public static byte MinimumDaysInMonth => 28;

        /// <summary>
        /// Gets the amount of days in the month using <see cref="BaseYear"/>
        /// </summary>
        /// <param name="month">The month</param>
        /// <returns>The amount of days in that month</returns>
        public static int DaysInMonth(Month month) {
            return DaysInMonth((int)month);
        }

        /// <summary>
        /// Gets the amount of days in the month using <see cref="BaseYear"/>
        /// </summary>
        /// <param name="month">The index of the month</param>
        /// <returns>The amount of days in that month</returns>
        public static int DaysInMonth(int month) {
            return DateTime.DaysInMonth(BaseYear, month);
        }

        /// <summary>
        /// Gets the amount of days in the month of a year
        /// </summary>
        /// <param name="month">The month</param>
        /// <param name="year">The year</param>
        /// <returns>The amount of days in that month</returns>
        public static int DaysInMonth(Month month, int year) {
            return DaysInMonth((int)month, year);
        }

        /// <summary>
        /// Gets the amount of days in the year
        /// </summary>
        /// <param name="year">The index of the year</param>
        /// <returns>The amount of days in that year</returns>
        public static int DaysInYear(int year) {
            return new DateTime(year, 12, 31).DayOfYear;
        }

        /// <summary>
        /// Gets the amount of days in the month of a year
        /// </summary>
        /// <param name="month">The month</param>
        /// <param name="year">The year</param>
        /// <returns>The amount of days in that month</returns>
        public static int DaysInMonth(int month, int year) {
            return DateTime.DaysInMonth(year, month);
        }

        /// <summary>
        /// Attempts to parse a string value as date
        /// </summary>
        /// <param name="value">value to parse</param>
        /// <returns>The parsed date</returns>
        public static DateTime Parse(string value) {
            if (string.IsNullOrWhiteSpace(value)) {
                return BaseDate;
            }
            value = CleanValue(value);
            DateTime? result;
            foreach (var dateFormat in DateAndTimeFormats) {
                result = TryParse(value, dateFormat.Value, dateFormat.Key);
                if (result.HasValue) {
                    return result.Value;
                }
            }

            foreach (var dateFormat in ExtraDateFormats) {
                result = TryParse(value, dateFormat.Value, dateFormat.Key);
                if (result.HasValue) {
                    return result.Value;
                }
            }

            result = TryParse(value, new[] { "dd MMM  yyyy", "dd MMM yyyy" }, null);
            if (result.HasValue) {
                return result.Value;
            }

            if (DateTime.TryParse(value, out var parseResult)) {
                result = parseResult;
            }

            return result ?? BaseDate;
        }
        
        /// <summary>
        /// Gets the days between <see cref="from"/> and <see cref="to"/>.
        /// This includes both <see cref="from"/> if <see cref="includeFrom"/> is true (default).
        /// This includes both <see cref="to"/> if <see cref="includeTo"/> is true (default).
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="includeFrom"></param>
        /// <param name="includeTo"></param>
        /// <returns></returns>
        public static List<DateTime> GetDaysBetween(DateTime from, DateTime to, bool includeFrom = true, bool includeTo = true) {
            var days = new List<DateTime>();

            if (!includeFrom) {
                from = from.AddDays(1);
            }

            if (!includeTo) {
                to = to.AddDays(-1);
            }

            for (var day = from.Date; day.Date <= to.Date; day = day.AddDays(1)) {
                days.Add(day);
            }

            return days;
        }

        public static DateTime GetNextDateByDayOfWeek(DateTime current, DayOfWeek dayOfWeek) {
            var dates = Enumerable.Range(0, 7).Select(d => current.AddDays(d));
            return dates.Single(d => d.DayOfWeek == dayOfWeek);
        }

        private static string CleanValue(string value) {
            if (!value.Contains("MAART")) {
                if (value.Contains("MAAR")) {
                    value = value.Replace("MAAR", "MAR");
                } else if (value.Contains("MAA")) {
                    value = value.Replace("MAA", "MAR");
                }
            }
            if (value.Contains("AVR")) {
                value = value.Replace("AVR", "APR");
            }
            if (value.Contains("FEV")) {
                value = value.Replace("FEV", "FEB");
            }
            if (value.Contains("MARS")) {
                value = value.Replace("MARS", "MAR");
            }
            if (value.Contains("MӒR")) {
                value = value.Replace("MӒR", "MAR");
            }
            if (value.Contains("MÄR")) {
                value = value.Replace("MÄR", "MAR");
            }
            if (value.Contains("AVR")) {
                value = value.Replace("AVR", "APR");
            }
            if (value.Contains("MAI")) {
                value = value.Replace("MAI", "MEI");
            }
            if (value.Contains("JUIN")) {
                value = value.Replace("JUIN", "JUN");
            }
            if (value.Contains("JUIL")) {
                value = value.Replace("JUIL", "JUL");
            }
            if (value.Contains("AOUT")) {
                value = value.Replace("AOUT", "AUG");
            }
            if (value.Contains("SEPT")) {
                value = value.Replace("SEPT", "SEP");
            }
            if (value.Contains("OCT")) {
                value = value.Replace("OCT", "OKT");
            }
            if (value.Contains("NOV")) {
                value = value.Replace("NOV", "NOV");
            }
            if (value.Contains("DEZ".ToUpper())) {
                value = value.Replace("DEZ", "DEC");
            }
            return value;
        }

        private static DateTime? TryParse(string value, string[] formats, CultureInfo cultureInfo) {
            if (DateTime.TryParseExact(value, formats, cultureInfo, DateTimeStyles.None, out var parseResult)) {
                return parseResult;
            }
            return null;
        }

        private static readonly Dictionary<CultureInfo, string[]> DateFormats = new Dictionary<CultureInfo, string[]> {
            {
                CultureInfo.CurrentUICulture,
                new[] {
                    "dd/MM/yyyy", "d/MM/yyyy", "dd/M/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-MM-yyyy", "dd-M-yyyy",
                    "d-M-yyyy", "ddMMyyyy", "dMMyyyy", "ddMyyyy", "dMyyyy", "dd MMM  yyyy", "dd MMM yyyy",
                    "ddMMyy", "yyyy-MM-dd", "yyyy/MM/dd", "yyyyMMdd",
                }
            }
        };

        private static readonly Dictionary<CultureInfo, string[]> TimeFormats = new Dictionary<CultureInfo, string[]> {
            {
                CultureInfo.CurrentUICulture,
                new[] {
                    "hh:mm:ss tt", "hh:mm tt", "HH:mm:ss", "HH:mm", "hh:m:ss tt", "hh:m tt", "HH:m:ss", "HH:m",
                    "h:mm:ss tt", "h:mm tt", "H:mm:ss", "H:mm", "h:m:ss tt", "h:m tt", "H:m:ss", "H:m",
                    "hh:mm:s tt", "HH:mm:s", "hh:m:s tt", "HH:m:s", "h:mm:s tt", "H:mm:s", "h:m:s tt", "H:m:s",
                    "hhmmss", "HHmmss", "hhmm", "HHmm",
                }
            }
        };

        private static readonly Dictionary<CultureInfo, string[]> DateAndTimeFormats = FillDateAndTimeFormats();

        private static Dictionary<CultureInfo, string[]> FillDateAndTimeFormats() {
            var result = new Dictionary<CultureInfo, string[]>();
            foreach (var dateFormat in DateFormats) {
                var timeFormats = TimeFormats.TryGetValueWithDefault(dateFormat.Key, new[] { "" });
                var formats = new List<string>(dateFormat.Value.Length * timeFormats.Length);
                foreach (var dateFormatValue in dateFormat.Value) {
                    foreach (var timeFormat in timeFormats) {
                        formats.Add((dateFormatValue + " " + timeFormat).Trim());
                        if (dateFormatValue.All(char.IsLetter) && timeFormat.All(char.IsLetter)) {
                            formats.Add((dateFormatValue + timeFormat).Trim());
                        }
                    }
                    formats.Add(dateFormatValue);
                }
                result.Add(dateFormat.Key, formats.ToArray());
            }
            return result;
        }

        private static readonly Dictionary<CultureInfo, string[]> ExtraDateFormats =
            new Dictionary<CultureInfo, string[]> {
                { CultureInfo.GetCultureInfo("fr-FR"), new[] { "dd MMM  yyyy", "dd MMM yyyy", "dd.MMM .yyyy", "dd.MMM.yyyy", "dd MMMM  yyyy", "dd MMMM yyyy", "dd.MMMM .yyyy", "dd.MMMM.yyyy", } },
                { CultureInfo.GetCultureInfo("nl-BE"), new[] { "dd MMM  yyyy", "dd MMM yyyy", "dd.MMM .yyyy", "dd.MMM.yyyy", "dd MMMM  yyyy", "dd MMMM yyyy", "dd.MMMM .yyyy", "dd.MMMM.yyyy", } },
                { CultureInfo.InvariantCulture, new[] { "dd MMM  yyyy", "dd MMM yyyy", "dd.MMM .yyyy", "dd.MMM.yyyy", "dd MMMM  yyyy", "dd MMMM yyyy", "dd.MMMM .yyyy", "dd.MMMM.yyyy", } }
            };
    }
}
