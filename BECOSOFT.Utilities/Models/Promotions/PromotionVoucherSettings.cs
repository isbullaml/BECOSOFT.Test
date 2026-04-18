using System.Collections.Generic;

namespace BECOSOFT.Utilities.Models.Promotions {
    public class PromotionVoucherSettings {
        public bool ExcludeDefaultCustomerFromVoucherAction { get; set; }
        public List<int> ExcludedDocumentTypeIDsFromVoucherAction { get; set; }
        public int VoucherArticleID { get; set; }
    }
    public class PromotionActionVoucherSettings {
        public int NumberOfMonthsValidAfterPromotionEnds { get; set; }
    }
}