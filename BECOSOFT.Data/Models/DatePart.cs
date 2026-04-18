using BECOSOFT.Utilities.Attributes;
using BECOSOFT.Utilities.Extensions;

namespace BECOSOFT.Data.Models {
    /// <summary>
    /// Possible SQL DATEPART arguments. If multiple abbreviations exist, the first one is always used (see comment on enum member).
    /// </summary>
    public enum DatePart {
        /// <summary>
        /// Year (yy, yyyy)
        /// </summary>
        [Abbreviation("yy")]
        Year,

        /// <summary>
        /// Quarter (qq, q)
        /// </summary>
        [Abbreviation("qq")]
        Quarter,

        /// <summary>
        /// Month (mm, m)
        /// </summary>
        [Abbreviation("mm")]
        Month,

        /// <summary>
        /// DayOfYear (dy)
        /// </summary>
        [Abbreviation("dy")]
        DayOfYear,

        /// <summary>
        /// Day (dd, d)
        /// </summary>
        [Abbreviation("dd")]
        Day,

        /// <summary>
        /// Week (wk, ww)
        /// </summary>
        [Abbreviation("wk")]
        Week,

        /// <summary>
        /// Weekday (dw)
        /// </summary>
        [Abbreviation("dw")]
        WeekDay,

        /// <summary>
        /// Hour (hh)
        /// </summary>
        [Abbreviation("hh")]
        Hour,

        /// <summary>
        /// Minute (mi, n)
        /// </summary>
        [Abbreviation("mi")]
        Minute,

        /// <summary>
        /// Second (ss, s)
        /// </summary>
        [Abbreviation("ss")]
        Second,

        /// <summary>
        /// Millisecond (ms)
        /// </summary>
        [Abbreviation("ms")]
        Millisecond,

        /// <summary>
        /// Microsecond (mcs)
        /// </summary>
        [Abbreviation("mcs")]
        Microsecond,

        /// <summary>
        /// Nanosecond (ns)
        /// </summary>
        [Abbreviation("ns")]
        Nanosecond,

        /// <summary>
        /// Tzoffset (tz)
        /// </summary>
        [Abbreviation("tz")]
        TimeZoneOffset,

        /// <summary>
        /// IsoWeek (isowk, isoww)
        /// </summary>
        [Abbreviation("isowk")]
        IsoWeek,
    }

    public static class DatePartExtensions {
        public static string GetAbbreviation(this DatePart datePart) {
            var abbr = datePart.GetAttribute<AbbreviationAttribute>();
            return abbr?.Abbreviation;
        }
    }
}