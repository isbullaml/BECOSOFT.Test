using System.Collections.Generic;

namespace BECOSOFT.Utilities.Models.Promotions {
    public class PromotionRecognitionResult {
        public List<RecognizedArticle> Recognitions { get; set; } = new List<RecognizedArticle>();
    }
}