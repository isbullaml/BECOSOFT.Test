using BECOSOFT.Utilities.Collections;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Models.Promotions;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Promotions {
    internal static class PromotionEligibleArticleChecker {
        public static PromotionEligibleArticlesResult GetEligibleArticles(PromotionEligibleArticlesParameters eligibleArticlesParameters) {
            // 1. Create a list containing the different quantity groups and their quantity (multiplied by criterion amount)
            var noActionArticleType = eligibleArticlesParameters.ActionArticleType == 0;
            var groups = new List<KeyValueList<List<EligiblePromotionArticle>, int>>();
            var allFrom = true;
            foreach (var groupedConditionResult in eligibleArticlesParameters.ConditionResultToProcess.GroupBy(c => c.Criterion.Group)) {
                var groupPerCriterion = groupedConditionResult.GroupBy(c => c.Criterion).ToList();
                //var quantityPerGroup = groupedConditionResult.
                var temp = new KeyValueList<List<EligiblePromotionArticle>, int>();
                foreach (var grouping in groupPerCriterion) {
                    var criterion = grouping.Key;
                    var isPer = criterion.Grouping == PromotionGrouping.Per;
                    foreach (var conditionResult in grouping) {
                        if (conditionResult.QuantityGroups.IsEmpty()) { continue; }
                        foreach (var quantityGroup in conditionResult.QuantityGroups) {
                            var criterionAmount = criterion.PerAmount.To<int>();
                            if (noActionArticleType) {
                                var eligibleCount = isPer ? eligibleArticlesParameters.NumberOfTimes * criterionAmount : quantityGroup.Key.Count;
                                temp.Add(quantityGroup.Key.Select(pa => new EligiblePromotionArticle {
                                    ArticleData = pa,
                                }).ToList(), eligibleCount);
                            } else {
                                var eligibleCount = quantityGroup.Value * (isPer ? criterionAmount : eligibleArticlesParameters.Quantity);
                                temp.Add(quantityGroup.Key.Select(pa => new EligiblePromotionArticle {
                                    ArticleData = pa,
                                }).ToList(), eligibleCount);
                            }
                        }
                    }
                    if (allFrom && isPer) { allFrom = false; }
                }
                groups.Add(temp);
            }
            // 2. Create a list of the different conditions, per condition list, sort and limit the eligible items
            var eligibleGroups = new List<List<List<EligiblePromotionArticle>>>();
            foreach (var grouping in groups) {
                var tempEligible = new List<List<EligiblePromotionArticle>>();
                var sortList = new List<EligiblePromotionArticle>();
                foreach (var group in grouping) {
                    // sort all articles within a grouping by the defined action article type (and limit by the quantity)
                    var sorted = PromotionArticleTypeSorter.Sort(group.Key, eligibleArticlesParameters.PrimaryActionArticleType, group.Value);
                    sortList.Add(sorted[0]);
                    tempEligible.Add(sorted);
                }
                // sort all eligible groups by the defined action article type
                var sortedSortList = PromotionArticleTypeSorter.Sort(sortList, eligibleArticlesParameters.PrimaryActionArticleType, int.MaxValue);
                tempEligible = tempEligible.OrderBy(el => sortedSortList.IndexOf(el[0])).ToList();
                eligibleGroups.Add(tempEligible);
            }
            short promotionGroupingIndex = 1;
            var eligibleArticles = new List<List<EligiblePromotionArticle>>();
            if (eligibleArticlesParameters.NumberOfTimes > 1 && eligibleGroups.Any(e => e.Count < eligibleArticlesParameters.NumberOfTimes)) {
                var tempEligibleGroups = new List<List<List<EligiblePromotionArticle>>>();
                foreach (var eligibleGroup in eligibleGroups) {
                    if (eligibleGroup.Count >= eligibleArticlesParameters.NumberOfTimes) {
                        tempEligibleGroups.Add(eligibleGroup);
                        continue;
                    }
                    foreach (var groupData in eligibleGroup) {
                        var t = new List<List<EligiblePromotionArticle>>();
                        var partitionSize = groupData.Count / eligibleArticlesParameters.NumberOfTimes;
                        foreach (var partition in groupData.Partition(partitionSize)) {
                            var eligiblePromotionArticles = partition.ToList();
                            if (eligibleArticlesParameters.EnablePromotionGroupingIndex) {
                                foreach (var eligiblePromotionArticle in eligiblePromotionArticles) {
                                    eligiblePromotionArticle.PromotionGroupingIndex = promotionGroupingIndex;
                                }
                                promotionGroupingIndex++;
                            }
                            t.Add(eligiblePromotionArticles);
                        }
                        tempEligibleGroups.Add(t);
                    }
                }
                eligibleGroups = tempEligibleGroups;
            }
            var toMark = new HashSet<EligiblePromotionArticle>();
            for (var i = 0; i < eligibleArticlesParameters.NumberOfTimes; i++) {
                // create a list containing eligible group from each condition
                var itemList = new List<EligiblePromotionArticle>();
                foreach (var eligibleGroup in eligibleGroups) {
                    if (eligibleGroup.IsEmpty()) { continue; }
                    if (allFrom) {
                        itemList.AddRange(eligibleGroup.SelectMany(itms => itms));
                    } else {
                        if (i >= eligibleGroup.Count) { continue; }
                        itemList.AddRange(eligibleGroup[i]);
                    }
                }
                // sort all articles before taking the required actionQuantity (number of articles to process)
                var sortedItemList = PromotionArticleTypeSorter.Sort(itemList, eligibleArticlesParameters.ActionArticleType, eligibleArticlesParameters.Quantity);
                toMark.AddRange(itemList.Except(sortedItemList));
                eligibleArticles.Add(sortedItemList);
            }
            return new PromotionEligibleArticlesResult {
                Included = eligibleArticles,
                ToMark = toMark.ToList(),
                AllFrom = allFrom,
            };
        }
    }
}