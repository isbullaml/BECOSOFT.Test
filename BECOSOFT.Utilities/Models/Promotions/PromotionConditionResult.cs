using BECOSOFT.Utilities.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace BECOSOFT.Utilities.Models.Promotions {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class PromotionConditionResult {
        public PromotionConditionWrapper Criterion { get; }
        /// <summary>
        /// <see cref="PromotionArticleData"/> objects that are valid for the <inheritdoc cref="Criterion"/>.
        /// </summary>
        public HashSet<PromotionArticleData> Included { get; set; } = new HashSet<PromotionArticleData>();
        /// <summary>
        /// <see cref="PromotionArticleData"/> objects that are invalid for the <inheritdoc cref="Criterion"/>.
        /// </summary>
        public HashSet<PromotionArticleData> Excluded { get; set; } = new HashSet<PromotionArticleData>();
        /// <summary>
        /// <see cref="PromotionArticleData"/> objects, grouped by the <see cref="PromotionConditionWrapper.Amount"/> property of the <see cref="Criterion"/>.
        /// </summary>
        public KeyValueList<HashSet<PromotionArticleData>, int> QuantityGroups { get; set; }
        /// <summary>
        /// Number of times the <see cref="Criterion"/> is valid.
        /// </summary>
        public int Times { get; set; }

        public PromotionConditionResult(PromotionConditionWrapper criterion) {
            Criterion = criterion;
        }

        public void AddItem(bool testResult, PromotionArticleData item) {
            if (testResult) {
                Included.Add(item);
            } else {
                Excluded.Add(item);
            }
        }

        public static PromotionConditionResult From(PromotionConditionResult result) {
            return new PromotionConditionResult(result.Criterion) {
                Included = result.Included,
                Excluded = result.Excluded,
                QuantityGroups = result.QuantityGroups,
                Times = result.Times,
            };
        }

        private string DebuggerDisplay => $"{Criterion?.Group}: {Criterion?.ConditionType}, Incl: {Included.Count}, Excl: {Excluded.Count}, Times: {Times}";
    }
}