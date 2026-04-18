using System;

namespace BECOSOFT.Utilities.Models.Promotions {
    public class PromotionAddVoucherWithDiscountAction {
        public int PromotionID { get; set; }
        public int VoucherArticleID { get; set; }
        public decimal Discount { get; set; }
        public decimal CalculatedDiscountValue { get; set; }

        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
    }
}