namespace BECOSOFT.Utilities.Models.Promotions {
    public class PromotionLocation {
        public int PromotionID { get; set; }
        public int WarehouseID { get; set; }

        public PromotionLocation Copy() {
            return new PromotionLocation {
                PromotionID = PromotionID,
                WarehouseID = WarehouseID,
            };
        }
    }
}