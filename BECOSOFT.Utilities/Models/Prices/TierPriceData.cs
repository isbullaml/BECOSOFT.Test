using System;

namespace BECOSOFT.Utilities.Models.Prices {
    public class TierPriceData : DiscountHolder {
        public int ArticleID { get; set; }
        public decimal Quantity { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal PercentageMultiplier { get; set; } = 1m;
        public bool IsPromo { get; set; }
        public int Rounding { get; set; }
        public int SupplierID { get; set; }
        public int VatGroupID { get; set; }
        public decimal VatPercentage { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public bool MarginPrice { get; set; }
        public bool PromoMarginPrice { get; set; }

        /// <summary>
        /// Range based on the current <see cref="From"/> and <see cref="To"/> values.
        /// </summary>
        public Range<DateTime> DateRange => new Range<DateTime>(From, To);

        public bool IsEmpty { get; private set; }

        public static TierPriceData Empty(int articleID) => new TierPriceData { ArticleID = articleID, Quantity = 1, IsEmpty = true };
    }
}