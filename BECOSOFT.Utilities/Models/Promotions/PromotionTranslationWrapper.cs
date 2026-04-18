namespace BECOSOFT.Utilities.Models.Promotions {
    public class PromotionTranslationWrapper {
        public int LanguageID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Notification { get; set; }

        public PromotionTranslationWrapper Copy() {
            return new PromotionTranslationWrapper {
                LanguageID = LanguageID,
                Name = Name,
                Description = Description,
                Notification = Notification,
            };
        }
    }
}