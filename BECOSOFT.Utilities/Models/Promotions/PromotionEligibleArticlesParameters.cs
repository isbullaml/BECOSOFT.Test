using System.Collections.Generic;

namespace BECOSOFT.Utilities.Models.Promotions {
    internal class PromotionEligibleArticlesParameters {
        public List<PromotionConditionResult> ConditionResultToProcess { get; }
        public int NumberOfTimes { get; }
        public PromotionActionArticleType ActionArticleType { get; set; }
        public PromotionActionArticleType PrimaryActionArticleType { get; set; }
        public int Quantity { get; set; }
        public bool EnablePromotionGroupingIndex { get; }

        public PromotionEligibleArticlesParameters(List<PromotionConditionResult> conditionResultToProcess, int numberOfTimes, bool enablePromotionGroupingIndex) {
            ConditionResultToProcess = conditionResultToProcess;
            NumberOfTimes = numberOfTimes;
            EnablePromotionGroupingIndex = enablePromotionGroupingIndex;
        }
    }
}