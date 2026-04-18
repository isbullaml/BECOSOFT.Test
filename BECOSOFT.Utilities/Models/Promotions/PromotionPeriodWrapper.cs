namespace BECOSOFT.Utilities.Models.Promotions {
    public class PromotionPeriodWrapper {
        public int PromotionID { get; set; }
        public int PeriodID { get; set; }

        public PromotionPeriodWrapper Copy() {
            return new PromotionPeriodWrapper {
                PromotionID = PromotionID,
                PeriodID = PeriodID,
            };
        }
    }
}