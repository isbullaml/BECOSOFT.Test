using System.Collections.Generic;

namespace BECOSOFT.Utilities.Models.Promotions {
    public class PromotionSettings {
        public int DefaultCustomerID { get; set; }
        public List<int> ExcludedDocumentTypeIDs { get; set; }
        public List<int> ExcludedContactIDs { get; set; }

        public bool SkipWithDiscount { get; set; }
        public bool SkipWithPromotionPrice { get; set; }
        public bool SkipNoDiscountArticle { get; set; }
        public bool SkipGiftVoucher { get; set; }
        public bool SkipDiscountVoucher { get; set; }
        public bool IncludePriceChangedGenericArticle { get; set; }
        public bool SkipNoPoints { get; set; }
        public bool IncludeWeightedArticles { get; set; }

        public PromotionActionVoucherSettings ActionVoucherSettings { get; set; }
        public PromotionVoucherSettings VoucherSettings { get; set; }
    }
}
