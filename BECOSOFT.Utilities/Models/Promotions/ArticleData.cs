using System.Diagnostics;

namespace BECOSOFT.Utilities.Models.Promotions {
    /// <summary>
    /// This class is a minimal representation of a document detail line or a cart item.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ArticleData {
        /// <summary>
        /// The index of the detail line or cart item
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The article ID of the detail line or cart item
        /// </summary>
        public int ArticleID { get; set; }
        public bool SkipPromotionCalculation { get; set; }
        public bool IsGenericArticle { get; set; }
        public bool IsPriceChanged { get; set; }
        public bool IsNoDiscount { get; set; }
        public bool IsNoPoints { get; set; }
        public bool IsNoTierPrice { get; set; }
        public bool IsWeighted { get; set; }
        public bool IsFixedPrice { get; set; }
        public PromotionVoucherType VoucherType { get; set; }
        public decimal? VoucherValue { get; set; }
        public int PromotionID { get; set; }
        public bool IsAddedByPromotion { get; set; }
        public decimal PointFactor { get; set; }
        public decimal? Weight { get; set; }

        /// <summary>
        /// Quantity of the detail line or cart item
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// Quantity of the detail line (actual weight)
        /// </summary>
        public decimal? OriginalWeightedQuantity { get; set; }
        public decimal BasePrice { get; set; }
        public decimal Discount1 { get; set; }
        public decimal Discount2 { get; set; }
        public decimal Discount3 { get; set; }
        public decimal Discount4 { get; set; }
        public decimal Discount5 { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal? PriceWithoutPromotion { get; set; }

        /// <summary>
        /// The purchase price is used when <see cref="PromotionWrapper.NotBelowPurchasePrice"/> is set.
        /// </summary>
        public decimal? PurchasePrice { get; set; }

        /// <summary>
        /// Returns the calculated price based on <see cref="BasePrice"/>, <see cref="DiscountAmount"/> or <see cref="Discount1"/> to <see cref="Discount5"/>.
        /// </summary>
        public decimal Price {
            get {
                if (DiscountAmount != 0) {
                    return BasePrice - DiscountAmount;
                }
                return BasePrice * (1m - (Discount1 / 100m))
                                 * (1m - (Discount2 / 100m))
                                 * (1m - (Discount3 / 100m))
                                 * (1m - (Discount4 / 100m))
                                 * (1m - (Discount5 / 100m));
            }
        }

        public bool HasDiscount => DiscountAmount != 0 || Discount1 != 0 || Discount2 != 0 || Discount3 != 0 || Discount4 != 0 || Discount5 != 0;
        public bool HasPromotionPrice => PriceWithoutPromotion.HasValue && PriceWithoutPromotion != BasePrice;

        /// <summary>
        /// Create a clone of the current <see cref="ArticleData"/> object.
        /// </summary>
        /// <returns>A clone of the current <see cref="ArticleData"/> object.</returns>
        public ArticleData Clone() {
            var res = new ArticleData {
                Index = Index,
                ArticleID = ArticleID,
                Quantity = Quantity,
                BasePrice = BasePrice,
                Discount1 = Discount1,
                Discount2 = Discount2,
                Discount3 = Discount3,
                Discount4 = Discount4,
                Discount5 = Discount5,
                DiscountAmount = DiscountAmount,
                PriceWithoutPromotion = PriceWithoutPromotion,
                PurchasePrice = PurchasePrice,
                SkipPromotionCalculation = SkipPromotionCalculation,
                IsGenericArticle = IsGenericArticle,
                IsPriceChanged = IsPriceChanged,
                IsNoDiscount = IsNoDiscount,
                IsNoPoints = IsNoPoints,
                VoucherType = VoucherType,
                VoucherValue = VoucherValue,
                PromotionID = PromotionID,
                IsAddedByPromotion = IsAddedByPromotion,
                PointFactor = PointFactor,
                Weight = Weight,
                IsWeighted = IsWeighted,
                OriginalWeightedQuantity = OriginalWeightedQuantity,
            };
            return res;
        }

        private string DebuggerDisplay => $"Index: {Index}, ArtID: {ArticleID} x {Quantity}, HasDiscount? {HasDiscount}, HasPromotionPrice? {HasPromotionPrice}";
    }
}