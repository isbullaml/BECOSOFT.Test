using System;

namespace BECOSOFT.Utilities.Models.Accounting {
    public class AccountingPeriodBookYearDeviation {
        public Month? PeriodStartMonth { get; set; }
        public short? PeriodStartYear { get; set; }
        public Month? PeriodEndMonth { get; set; }
        public short? PeriodEndYear { get; set; }
        public bool IsQuarterly { get; set; }
        public short BookYear { get; set; }
        public Tuple<Month, short> PeriodStart => PeriodStartMonth == null || PeriodStartYear == null ? null : Tuple.Create(PeriodStartMonth.Value, PeriodStartYear.Value);
        public Tuple<Month, short> PeriodEnd => PeriodEndMonth == null || PeriodEndYear == null ? null : Tuple.Create(PeriodEndMonth.Value, PeriodEndYear.Value);
    }
}