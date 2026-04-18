using System.Collections.Generic;

namespace BECOSOFT.Utilities.Models.Promotions {
    public class PromotionCombinedConditionResult {
        public List<PromotionConditionResult> DefaultConditionResult { get; set; }
        public List<PromotionConditionResult> ActionConditionResult { get; set; }
        public bool HasConditionSpecificActions { get; set; }
        public int? OverrulingNumberOfTimes { get; set; }
    }
}