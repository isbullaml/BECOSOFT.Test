namespace BECOSOFT.Utilities.Models.Promotions {
    public class PromotionExcludedDocument {
        public int PromotionID { get; set; }
        public int DocumentTypeID { get; set; }

        public PromotionExcludedDocument Copy() {
            return new PromotionExcludedDocument {
                PromotionID = PromotionID,
                DocumentTypeID = DocumentTypeID,
            };
        }
    }
}