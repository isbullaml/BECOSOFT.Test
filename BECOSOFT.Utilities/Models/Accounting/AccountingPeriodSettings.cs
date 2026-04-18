using System.Collections.Generic;

namespace BECOSOFT.Utilities.Models.Accounting {
    public class AccountingPeriodSettings {
        public bool IsQuarterly { get; set; }
        public int BookYearOffset { get; set; }
        public Month StartMonthFirstPeriod { get; set; }
        public List<AccountingPeriodBookYearDeviation> BookYearDeviations { get; set; }

        public AccountingPeriodSettings() {
            BookYearDeviations = new List<AccountingPeriodBookYearDeviation>(0);
        }
    }
}