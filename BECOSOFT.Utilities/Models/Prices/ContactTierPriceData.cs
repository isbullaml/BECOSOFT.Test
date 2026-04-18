using System;

namespace BECOSOFT.Utilities.Models.Prices {
    public class ContactTierPriceData {
        public int ArticleID { get; set; }
        public decimal Quantity { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal Discount1 { get; set; }
        public decimal Discount2 { get; set; }
        public decimal Discount3 { get; set; }
        public decimal Discount4 { get; set; }
        public decimal Discount5 { get; set; }
        public decimal PercentageMultiplier { get; set; } = 1m;
        public bool IsPromo { get; set; }
        public int Rounding { get; set; }
        public int VatGroupID { get; set; }
        public decimal VatPercentage { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        /// <summary>
        /// Range based on the current <see cref="From"/> and <see cref="To"/> values.
        /// </summary>
        public Range<DateTime> DateRange => new Range<DateTime>(From, To);

        public bool HasDiscount => Discount1 != 0 || Discount1 != 0 || Discount2 != 0 || Discount3 != 0 || Discount4 != 0 || Discount5 != 0;

        public bool IsEmpty { get; private set; }

        public static ContactTierPriceData Empty(int articleID) => new ContactTierPriceData { ArticleID = articleID, Quantity = 1, IsEmpty = true };
    }
}