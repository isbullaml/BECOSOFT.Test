using BECOSOFT.Utilities.Models;
using System;

namespace BECOSOFT.Utilities.Helpers {
    public static class TimeHelper {

        public static double GetInterval(double interval, TimeInterval intervalUnit, Month? month = null, int? year = null) {
            return MillisecondsFrom(intervalUnit, month, year) * interval;
        }

        public static long GetInterval(int interval, TimeInterval intervalUnit, Month? month = null, int? year = null) {
            return MillisecondsFrom(intervalUnit, month, year) * interval;
        }

        private static long MillisecondsFrom(TimeInterval interval, Month? month = null, int? year = null) {
            long milliseconds = DurationMultiplier.MillisecondsPerMillisecond;
            if (interval > TimeInterval.Day) {
                if (interval == TimeInterval.Week) {
                    milliseconds *= DurationMultiplier.DaysPerWeek;
                } else if (interval == TimeInterval.Month) {
                    if (!month.HasValue) {
                        throw new ArgumentException($"Month is required when calculating for '{interval.ToString()}'",
                                                     nameof(month));
                    }
                    if (year.HasValue) {
                        milliseconds *= DurationMultiplier.DaysPerMonth(month.Value, year.Value);
                    } else {
                        milliseconds *= DurationMultiplier.DaysPerMonth(month.Value);
                    }
                } else if (interval == TimeInterval.Year) {
                    if (!year.HasValue) {
                        throw new ArgumentException($"Year is required when calculating for '{interval.ToString()}'",
                                                     nameof(month));
                    }
                    milliseconds *= DurationMultiplier.DaysPerYear(year.Value);
                }
            }
            if (interval > TimeInterval.Hour) {
                milliseconds *= DurationMultiplier.HoursPerDay;
            }
            if (interval > TimeInterval.Minute) {
                milliseconds *= DurationMultiplier.MinutesPerHour;
            }

            if (interval > TimeInterval.Second) {
                milliseconds *= DurationMultiplier.SecondsPerMinute;
            }

            if (interval > TimeInterval.Millisecond) {
                milliseconds *= DurationMultiplier.MillisecondsPerSecond;
            }

            return milliseconds;
        }
    }

    public enum TimeInterval {
        Millisecond,
        Second,
        Minute,
        Hour,
        Day,
        Week,
        Month,
        Year,
    }

    public static class DurationMultiplier {
        public const int MillisecondsPerMillisecond = 1;
        public const int MillisecondsPerSecond = 1000;
        public const int SecondsPerMinute = 60;
        public const int MinutesPerHour = 60;
        public const int HoursPerDay = 24;
        public const int DaysPerWeek = 7;
        public static int DaysPerMonth(Month month) => DateTimeHelpers.DaysInMonth(month);
        public static int DaysPerMonth(int month) => DateTimeHelpers.DaysInMonth(month);
        public static int DaysPerMonth(Month month, int year) => DateTimeHelpers.DaysInMonth(month, year);
        public static int DaysPerMonth(int month, int year) => DateTimeHelpers.DaysInMonth(month, year);
        public static int DaysPerYear(int year) => DateTimeHelpers.DaysInYear(year);
        public const int MonthsPerYear = 12;
    }
}