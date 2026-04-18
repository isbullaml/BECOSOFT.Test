using BECOSOFT.Utilities.Models.Promotions;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Promotions {
    internal static class CriterionChecker {
        internal static bool CheckCriterion(PromotionConditionWrapper criterion, PromotionArticleWrapper article) {
            var articleData = new PromotionArticleData(0, article.ArticleID, 0) {
                Data = article,
                Weight = article.Weight
            };
            return CheckCriterion(criterion, articleData);
        }

        internal static bool CheckCriterion(PromotionConditionWrapper criterion, PromotionArticleData articleData) {
            var article = articleData.Data;
            switch (criterion.ConditionType) {
                case PromotionConditionType.Brand:
                case PromotionConditionType.BrandValue:
                    return CheckValue(criterion, article.BrandID);
                case PromotionConditionType.DiscountGroup:
                    return CheckValue(criterion, article.DiscountGroup);
                case PromotionConditionType.DocumentValue:
                case PromotionConditionType.DocumentValueExcludingOtherPromotion:
                    return true;
                case PromotionConditionType.AllArticles:
                    return criterion.TypeOperator != PromotionTypeOperator.DifferentFrom;
                case PromotionConditionType.Group:
                case PromotionConditionType.ProductValue:
                    return CheckGroupStructure(criterion, article);
                case PromotionConditionType.Tag:
                    return CheckTag(criterion, article.TagIDs);
                case PromotionConditionType.FashionColor:
                    return CheckValue(criterion, article.FashionColor);
                case PromotionConditionType.FashionCollection:
                    return CheckValue(criterion, article.FashionCollection);
                case PromotionConditionType.FashionFabric:
                    return CheckValue(criterion, article.FashionFabric);
                case PromotionConditionType.Weight:
                    return articleData.Weight > 0 && criterion.Grouping == PromotionGrouping.From;
                case PromotionConditionType.Season:
                    return CheckValue(criterion, article.SeasonID);
                case PromotionConditionType.Matrix:
                    return CheckValue(criterion, article.MatrixID);
                case PromotionConditionType.PromotionArticleFilter:
                    var func = FilterConditionHelper.GenerateFunction<PromotionArticleData>(criterion);
                    return func(articleData);
                case PromotionConditionType.Type:
                    return CheckValue(criterion, article.Type);
                case PromotionConditionType.ArticleID:
                default:
                    return CheckValue(criterion, article.ArticleID);
            }
        }

        private static bool CheckTag(PromotionConditionWrapper criterion, HashSet<int> articleTagIDs) {
            var result = criterion.ValueSet.Any(articleTagIDs.Contains);
            if (criterion.TypeOperator == PromotionTypeOperator.DifferentFrom) {
                result = !result;
            }
            return result;
        }

        private static bool CheckGroupStructure(PromotionConditionWrapper criterion, PromotionArticleWrapper article) {
            var groupValues = criterion.GroupValues;
            // loop per column over group 1 -> 5 to search for a match
            for (var counter = 0; counter < groupValues[0].Count; counter++) {
                var match = true;
                for (var groupLevel = criterion.MinimumGroupLevel; groupLevel <= criterion.MaximumGroupLevel; groupLevel++) {
                    var conditionGroupID = groupValues[groupLevel - 1][counter];
                    if (conditionGroupID == 0) { break; }
                    if (conditionGroupID != article.GetGroupID(groupLevel)) {
                        match = false;
                        break;
                    }
                }
                if (match) {
                    return criterion.TypeOperator == PromotionTypeOperator.EqualTo;
                }
            }
            return criterion.TypeOperator == PromotionTypeOperator.DifferentFrom;
        }

        private static bool CheckValue(PromotionConditionWrapper criterion, int valueToCheck) {
            var result = criterion.ValueSet.Contains(valueToCheck);
            if (criterion.TypeOperator == PromotionTypeOperator.DifferentFrom) {
                result = !result;
            }
            return result;
        }

        private static bool CheckValue(PromotionConditionWrapper criterion, string valueToCheck) {
            var result = criterion.StringValueSet.Contains(valueToCheck);
            if (criterion.TypeOperator == PromotionTypeOperator.DifferentFrom) {
                result = !result;
            }
            return result;
        }
    }
}
