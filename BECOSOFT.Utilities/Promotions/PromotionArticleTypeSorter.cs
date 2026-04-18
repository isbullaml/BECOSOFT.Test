using BECOSOFT.Utilities.Models.Promotions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Promotions {
    internal static class PromotionArticleTypeSorter {
        public static List<EligiblePromotionArticle> Sort(List<EligiblePromotionArticle> items, PromotionActionArticleType actionArticleType,
                                                      int takeCount) {
            List<EligiblePromotionArticle> orderedArticles;
            switch (actionArticleType) {
                case PromotionActionArticleType.Cheapest:
                    orderedArticles = items.OrderBy(a => a.ArticleData.Price).ThenBy(a => a.ArticleData.ID).Take(takeCount).ToList();
                    break;
                case PromotionActionArticleType.MostExpensive:
                    orderedArticles = items.OrderByDescending(a => a.ArticleData.Price).ThenBy(a => a.ArticleData.ID).Take(takeCount).ToList();
                    break;
                case PromotionActionArticleType.MostExpensiveLine:
                case PromotionActionArticleType.MostExpensiveLineConsolidated:
                    Func<EligiblePromotionArticle, int> groupingFunc;
                    if (actionArticleType == PromotionActionArticleType.MostExpensiveLine) {
                        groupingFunc = a => a.ArticleData.Index;
                    } else {
                        groupingFunc = a => a.ArticleData.ArticleID;
                    }
                    var groupedByFunc = items.GroupBy(groupingFunc).OrderByDescending(a => a.Sum(aa => aa.ArticleData.Price * aa.ArticleData.Quantity));
                    var tempItems = new List<EligiblePromotionArticle>();
                    var remainingTakeCount = takeCount;
                    foreach (var grouping in groupedByFunc) {
                        if (remainingTakeCount == 0) { break; }
                        var promotionArticleDataInGrouping = grouping.ToList();
                        tempItems.AddRange(promotionArticleDataInGrouping);
                        remainingTakeCount -= 1;
                    }
                    orderedArticles = tempItems.ToList();
                    break;
                default:
                    orderedArticles = items.OrderBy(a => a.ArticleData.ID).Take(takeCount).ToList();
                    break;
            }
            return orderedArticles;
        }
        public static List<PromotionArticleData> Sort(List<PromotionArticleData> items, PromotionActionArticleType actionArticleType,
                                                      int takeCount) {
            List<PromotionArticleData> orderedArticles;
            switch (actionArticleType) {
                case PromotionActionArticleType.Cheapest:
                    orderedArticles = items.OrderBy(a => a.Price).ThenBy(a => a.ID).Take(takeCount).ToList();
                    break;
                case PromotionActionArticleType.MostExpensive:
                    orderedArticles = items.OrderByDescending(a => a.Price).ThenBy(a => a.ID).Take(takeCount).ToList();
                    break;
                case PromotionActionArticleType.MostExpensiveLine:
                case PromotionActionArticleType.MostExpensiveLineConsolidated:
                    Func<PromotionArticleData, int> groupingFunc;
                    if (actionArticleType == PromotionActionArticleType.MostExpensiveLine) {
                        groupingFunc = a => a.Index;
                    } else {
                        groupingFunc = a => a.ArticleID;
                    }
                    var groupedByFunc = items.GroupBy(groupingFunc).OrderByDescending(a => a.Sum(aa => aa.Price * aa.Quantity));
                    var tempItems = new List<PromotionArticleData>();
                    var remainingTakeCount = takeCount;
                    foreach (var grouping in groupedByFunc) {
                        if (remainingTakeCount == 0) { break; }
                        var promotionArticleDataInGrouping = grouping.ToList();
                        tempItems.AddRange(promotionArticleDataInGrouping);
                        remainingTakeCount -= 1;
                    }
                    orderedArticles = tempItems.ToList();
                    break;
                default:
                    orderedArticles = items.OrderBy(a => a.ID).Take(takeCount).ToList();
                    break;
            }
            return orderedArticles;
        }
    }
}