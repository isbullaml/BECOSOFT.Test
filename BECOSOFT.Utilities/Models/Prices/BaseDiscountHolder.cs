namespace BECOSOFT.Utilities.Models.Prices {
    public abstract class BaseDiscountHolder : IDiscountHolder {
        public static readonly int MinDiscountIndex = 1;
        public static readonly int MaxDiscountIndex = 5;
        public abstract decimal Discount1 { get; set; }
        public abstract decimal Discount2 { get; set; }
        public abstract decimal Discount3 { get; set; }
        public abstract decimal Discount4 { get; set; }
        public abstract decimal Discount5 { get; set; }

        public bool HasDiscount => this.GetHasDiscount();

        public decimal DiscountMultiplier => this.GetDiscountMultiplier();
    }
}