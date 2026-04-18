using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Models.Promotions;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Promotions {
    public static class PromotionCalculationHelpers {
        /// <summary>
        /// Calculate the minimum number of times the conditions of a promotion are present based on a <see cref="List{T}"/> of <see cref="PromotionConditionResult"/> objects.
        /// </summary>
        /// <param name="conditionResult"></param>
        /// <returns></returns>
        public static int? CalculateNumberOfTimes(List<PromotionConditionResult> conditionResult) {
            // group all criteria per condition
            var grouping = conditionResult.Where(c => c.Criterion.Grouping == PromotionGrouping.Per).GroupBy(c => c.Criterion.Group).ToList();
            // sum each times per grouping
            var timesPerCondition = grouping.Select(conditionGrouping => new int?(conditionGrouping.Sum(c => c.Times))).ToList();
            // return the lowest times value
            return timesPerCondition.MinOrDefault(times => times);
        }
    }
}