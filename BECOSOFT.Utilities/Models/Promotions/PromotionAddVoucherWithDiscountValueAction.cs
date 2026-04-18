using System;

namespace BECOSOFT.Utilities.Models.Promotions {
    public class PromotionAddVoucherWithDiscountValueAction {
        public int PromotionID { get; set; }
        public int VoucherArticleID { get; set; }
        public decimal DiscountValue { get; set; }

        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
    }
}