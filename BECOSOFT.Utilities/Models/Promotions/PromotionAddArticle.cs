namespace BECOSOFT.Utilities.Models.Promotions {
    public abstract class PromotionAddArticle {
        public int PromotionID { get; set; }
        public int ArticleID { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuantityAlreadyPresent { get; set; }
        /// <summary>
        /// Calculated Total (<see cref="Quantity"/> + <see cref="QuantityAlreadyPresent"/>).
        /// </summary>
        public decimal TotalQuantity => Quantity + QuantityAlreadyPresent;
    }
}