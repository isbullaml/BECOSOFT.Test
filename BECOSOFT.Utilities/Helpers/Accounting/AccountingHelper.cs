using BECOSOFT.Utilities.Collections;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Extensions.Numeric;
using BECOSOFT.Utilities.Models;
using BECOSOFT.Utilities.Models.Accounting;
using System;
using System.Linq;

namespace BECOSOFT.Utilities.Helpers.Accounting {
    public static class AccountingHelper {
        private static readonly byte[] DefaultQuarterDefinition = { 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4 };
        private static readonly byte[] DefaultMonthDefinition = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

        private class AccountingPeriodProperties {
            public Month Month { get; set; }
            public short Year { get; set; }
            public Month StartMonth { get; set; }
            public short StartYear { get; set; }
            public Month EndMonth { get; set; }
            public short EndYear { get; set; }
            public bool IsQuarterly { get; set; }
            public int BookYear { get; set; }
        }

        public static AccountingPeriod GetAccountingPeriod(int year, Month month, AccountingPeriodSettings accountingPeriodSettings) {
            if (month == 0 || (Month) month > Month.December) {
                throw new ArgumentException();
            }
            var date = new DateTime(year, (int) month, 1);
            return GetAccountingPeriod(date, accountingPeriodSettings);
        }

        public static AccountingPeriod GetAccountingPeriod(DateTime date, AccountingPeriodSettings accountingPeriodSettings) {
            var year = (short) date.Year;
            var month = date.GetMonth();
            AccountingPeriodProperties properties = null;
            if (accountingPeriodSettings.BookYearDeviations.HasAny()) {
                foreach (var bookYearDeviation in accountingPeriodSettings.BookYearDeviations) {
                    var periodStart = bookYearDeviation.PeriodStart;
                    var periodEnd = bookYearDeviation.PeriodEnd;
                    var startYear = periodStart?.Item2 ?? periodEnd?.Item2 ?? 0;
                    var endYear = periodEnd?.Item2 ?? periodStart?.Item2 ?? 0;
                    var monthPeriods = new KeyValueList<Month, short>();
                    var firstMonth = periodStart?.Item1.NullIf((Month) 0) ?? Month.January;
                    var lastMonth = periodEnd?.Item1 ?? Month.December;
                    var currentMonth = (int)firstMonth;
                    var currentYear = startYear;
                    do {
                        do {
                            monthPeriods.Add((Month) currentMonth, currentYear);
                            currentMonth += 1;
                        } while (currentMonth != (int) Month.December);
                        monthPeriods.Add((Month) currentMonth, currentYear);
                        currentYear += 1;
                        currentMonth = 1;
                    } while (currentYear != endYear + 1);
                    var monthPair = KeyValuePair.Create(month, year);
                    if (!monthPeriods.Contains(monthPair)) { continue; }
                    properties = new AccountingPeriodProperties {
                        Year = year,
                        Month = month,
                        StartMonth = firstMonth,
                        StartYear = startYear,
                        EndMonth = lastMonth,
                        EndYear = endYear,
                        IsQuarterly = bookYearDeviation.IsQuarterly,
                        BookYear = bookYearDeviation.BookYear,
                    };
                }
            }
            if (properties == null) {
                var startMonth = accountingPeriodSettings.StartMonthFirstPeriod.NullIf((Month) 0) ?? Month.January;
                var endMonth = startMonth == Month.January ? Month.December : (Month) ((int) startMonth - 1);
                var monthRange = Models.RangeHelper.Create((int) startMonth, (int) Month.December);
                var bookYear = year + accountingPeriodSettings.BookYearOffset;
                var startYear = year;
                var endYear = year;
                if (monthRange.Excludes((int) month)) {
                    bookYear -= 1;
                    startYear -= 1;
                } else if(startMonth != Month.January) {
                    endYear += 1;
                }
                properties = new AccountingPeriodProperties {
                    Year = year,
                    Month = month,
                    StartMonth = startMonth,
                    StartYear = startYear,
                    EndMonth = endMonth,
                    EndYear = endYear,
                    IsQuarterly = accountingPeriodSettings.IsQuarterly,
                    BookYear = bookYear,
                };
            }
            return GetAccountingPeriod(properties);
        }

        private static AccountingPeriod GetAccountingPeriod(AccountingPeriodProperties properties) {
            int period;
            int numberOfMonths;
            if (properties.IsQuarterly) {
                int index;
                var shift = 0;
                if (properties.StartYear == properties.EndYear) {
                    // 6 maanden
                    // 0  1  2  3  4  5
                    // 7  8  9 10 11 12
                    // 1  1  1  2  2  2
                    // 9 maanden
                    // 0  1  2  3  4  5  6  7  8
                    // 4  5  6  7  8  9 10 11 12
                    // 1  1  1  2  2  2  3  3  3
                    numberOfMonths = (int) properties.EndMonth - (int) properties.StartMonth + 1;
                    index = (int) properties.Month - (12 - numberOfMonths) - 1;
                } else {
                    numberOfMonths = (int) Month.December - (int) properties.StartMonth + (int) properties.EndMonth + 1;
                    index = (int) properties.Month - 1;
                    if (properties.Year == properties.EndYear) {
                        if (numberOfMonths > 12) {
                            shift = 12;
                        } else {
                            shift = (int) properties.StartMonth - 1;
                        }
                    }else if (properties.Year == properties.StartYear) {
                        index = (int) properties.Month - (int) properties.StartMonth;
                    }
                    index %= numberOfMonths;
                }
                var periods = numberOfMonths / 3;
                var seed = Enumerable.Range(1, periods).SelectMany(p => Enumerable.Repeat(p, 3)).ToArray();
                period = seed.GetShiftedValue(shift, index);
                //period = seed[index];
            } else {
                if (properties.StartYear == properties.EndYear) {
                    // 6 maanden
                    // 0  1  2  3  4  5
                    // 7  8  9 10 11 12
                    // 1  2  3  4  5  6
                    // 8 maanden
                    // 0  1  2  3  4  5  6  7
                    // 5  6  7  8  9 10 11 12
                    // 1  2  3  4  5  6  7  8
                    // 10 
                    numberOfMonths = (int) properties.EndMonth - (int) properties.StartMonth + 1;
                    period = (int) properties.Month - (12 - numberOfMonths);
                } else {
                    period = (int) properties.Month - (int) properties.StartMonth + 1;
                    if (properties.Year == properties.EndYear) {
                        period += 12;
                    }
                }
            }
            var bookYear = properties.BookYear;
            return new AccountingPeriod(bookYear, properties.IsQuarterly, (byte) period);
        }

        /// <summary>
        /// Gets the <see cref="AccountingPeriod"/> information for the given parameters.
        /// </summary>
        /// <param name="year">base year</param>
        /// <param name="month"><see cref="Month"/> to calculate the accounting year for</param>
        /// <param name="yearOffset">year offset</param>
        /// <param name="isQuarterly">The accounting period uses quarters.</param>
        /// <param name="startMonth">Specify the first month of the first quarter or period to define the quarter or month offset.</param>
        /// <returns>Returns the calculated <see cref="AccountingPeriod"/>.</returns>
        public static AccountingPeriod GetAccountingPeriod(int year, Month month, int yearOffset,
                                                           bool isQuarterly = false,
                                                           Month startMonth = Month.January) {
            if (month == 0 || (Month) month > Month.December) {
                throw new ArgumentException();
            }
            var date = new DateTime(year, (int) month, 1);
            var accountingPeriodSettings = new AccountingPeriodSettings {
                IsQuarterly = isQuarterly,
                StartMonthFirstPeriod = startMonth,
                BookYearOffset = yearOffset,
            };
            return GetAccountingPeriod(date, accountingPeriodSettings);
        }

        /// <summary>
        /// Gets the <see cref="AccountingPeriod"/> information for the given parameters.
        /// </summary>
        /// <param name="date">base date</param>
        /// <param name="yearOffset">year offset</param>
        /// <param name="isQuarterly">The accounting period uses quarters.</param>
        /// <param name="startMonth">Specify the first month of the first quarter or period to define the quarter or month offset.</param>
        /// <returns>Returns the calculated <see cref="AccountingPeriod"/>.</returns>
        public static AccountingPeriod GetAccountingPeriod(DateTime date, int yearOffset, 
                                                           bool isQuarterly = false, 
                                                           Month startMonth = Month.January) {
            var accountingPeriodSettings = new AccountingPeriodSettings {
                IsQuarterly = isQuarterly,
                StartMonthFirstPeriod = startMonth,
                BookYearOffset = yearOffset,
            };
            return GetAccountingPeriod(date, accountingPeriodSettings);
        }


        /// <summary>
        /// Gets the accounting year of the provided <see cref="date"/> and <see cref="yearOffset"/>. By default the first month of the first quarter is <see cref="Month.January"/>.
        /// </summary>
        /// <param name="date">Base date</param>
        /// <param name="yearOffset">Year offset</param>
        /// <param name="startMonth">Specify the first month of the first quarter or period to define the quarter or month offset.</param>
        /// <returns>Returns the accounting year.</returns>
        public static int GetYear(DateTime date, int yearOffset, Month startMonth = Month.January) {
            var accountingPeriodSettings = new AccountingPeriodSettings {
                IsQuarterly = false,
                StartMonthFirstPeriod = startMonth,
                BookYearOffset = yearOffset,
            };
            var accountingPeriod = GetAccountingPeriod(date, accountingPeriodSettings);
            return accountingPeriod.Year;
        }

        public static int GetGeneralLedgerAccountLength(string countryCode) {
            var lowerCode = countryCode.ToLowerInvariant();
            if (lowerCode.Equals("be")) {
                return 6;
            }
            if (lowerCode.Equals("nl")) {
                return 4;
            }
            throw new ArgumentException("Unknown country code", nameof(countryCode));
        }
    }
}