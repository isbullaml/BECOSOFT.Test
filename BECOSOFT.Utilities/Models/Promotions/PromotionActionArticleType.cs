using BECOSOFT.Utilities.Attributes;

namespace BECOSOFT.Utilities.Models.Promotions {
    public enum PromotionActionArticleType {
        /// <summary>
        /// Cheapest line. Value: 1
        /// </summary>
        [LocalizedEnum(nameof(Resources.PromotionActionArticleType_Cheapest), NameResourceType = typeof(Resources))]
        Cheapest = 1,
        /// <summary>
        /// Most expensive. Value: 2
        /// </summary>
        [LocalizedEnum(nameof(Resources.PromotionActionArticleType_MostExpensive), NameResourceType = typeof(Resources))]
        MostExpensive = 2,
        /// <summary>
        /// Most expensive line. Value: 3
        /// </summary>
        [LocalizedEnum(nameof(Resources.PromotionActionArticleType_MostExpensiveLine), NameResourceType = typeof(Resources))]
        MostExpensiveLine = 3,
        /// <summary>
        /// Most expensive line (consolidated). Value: 4
        /// </summary>
        [LocalizedEnum(nameof(Resources.PromotionActionArticleType_MostExpensiveLineConsolidated), NameResourceType = typeof(Resources))]
        MostExpensiveLineConsolidated = 4,
    }
}