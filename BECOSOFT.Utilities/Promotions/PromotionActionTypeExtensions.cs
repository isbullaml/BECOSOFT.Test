using BECOSOFT.Utilities.Models.Promotions;

namespace BECOSOFT.Utilities.Promotions {
    public static class PromotionActionTypeExtensions {
        /// <summary>
        /// Returns whether the <param name="actionType"></param> is equal to <see cref="PromotionActionType.AddArticleWithDiscount"/> or <see cref="PromotionActionType.AddArticleWithPrice"/>
        /// </summary>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public static bool IsAddArticle(this PromotionActionType actionType) {
            return actionType == PromotionActionType.AddArticleWithDiscount || actionType == PromotionActionType.AddArticleWithPrice;
        }
    }
}