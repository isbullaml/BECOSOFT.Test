namespace BECOSOFT.Utilities.Models.Prices {
    public interface IDiscountHolder {
        decimal Discount1 { get; set; }
        decimal Discount2 { get; set; }
        decimal Discount3 { get; set; }
        decimal Discount4 { get; set; }
        decimal Discount5 { get; set; }
        bool HasDiscount { get; }

        /// <summary>
        /// All discounts (1-5) multiplied with each other.
        /// </summary>
        decimal DiscountMultiplier { get; }
    }
}