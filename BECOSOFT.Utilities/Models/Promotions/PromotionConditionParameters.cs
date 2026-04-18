using System.Collections.Generic;

namespace BECOSOFT.Utilities.Models.Promotions {
    internal class PromotionConditionParameters {
        public PromotionWrapper Promotion { get; set; }
        public PromotionConditionKind Kind { get; set; }
        public List<PromotionArticleData> Data { get; set; }
        public int MinimumGroupLevel { get; }
        public int MaximumGroupLevel { get; }

        public PromotionConditionParameters(int minimumGroupLevel, int maximumGroupLevel) {
            MinimumGroupLevel = minimumGroupLevel;
            MaximumGroupLevel = maximumGroupLevel;
        }
    }
}