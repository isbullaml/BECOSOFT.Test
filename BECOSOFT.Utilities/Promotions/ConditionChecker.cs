using BECOSOFT.Utilities.Collections;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Models.Promotions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BECOSOFT.Utilities.Promotions {
    internal static class ConditionChecker {
        public static List<PromotionConditionResult> CheckConditions(PromotionConditionParameters parameters) {
            /*
             Flow:
             1. Groeperen van condities per voorwaarde
             2. Per voorwaarde kijken welke artikelen er voldoen aan de verschillende criteria
             3. Per criteria kijken of er aan de aantal / waarde wordt voldaan
             4. Als niet aan alle condities voldaan is, geen resultaat geven.
             5. Artikels die eruit worden gefilterd (bij een Alle Artikelen criteria) uit de resultaat set halen 
             */

            // Step 1.
            var conditions = GetGroupedConditions(parameters, out var hasAllArticlesCriteria);
            if (conditions.IsEmpty()) { return null; }

            var conditionMatchCount = 0;
            var conditionCount = conditions.Count;
            var criterionContainer = new List<PromotionConditionResult>();
            foreach (var criteria in conditions) {
                // Step 2.
                foreach (var criterion in criteria) {
                    var conditionResult = new PromotionConditionResult(criterion);
                    criterionContainer.Add(conditionResult);
                    foreach (var article in parameters.Data) {
                        var testResult = CriterionChecker.CheckCriterion(criterion, article);
                        conditionResult.AddItem(testResult, article);
                    }
                }
                // Step 3.
                PerformQuantityCheck(criterionContainer);
            }
            var byCondition = criterionContainer.GroupBy(c => c.Criterion.Group).ToSafeList();
            foreach (var criterionResult in byCondition) {
                foreach (var result in criterionResult) {
                    if (result.Included.Count == 0) { continue; }
                    conditionMatchCount += 1;
                    break;
                }
            }
            // Step 4.
            if (conditionMatchCount != conditionCount) {
                return new List<PromotionConditionResult>(0);
            }
            // Step 5.
            if (hasAllArticlesCriteria) {
                var allArticlesConditionResult = criterionContainer.First(c => c.Criterion.ConditionType == PromotionConditionType.AllArticles);
                foreach (var criterionResult in criterionContainer) {
                    if (ReferenceEquals(criterionResult.Criterion, allArticlesConditionResult.Criterion)) { continue; }
                    var allIncludedItems = allArticlesConditionResult.Included;
                    var excludedItems = criterionResult.Excluded;
                    var intersected = new HashSet<PromotionArticleData>(allIncludedItems);
                    intersected.ExceptWith(excludedItems);
                    foreach (var item in excludedItems) {
                        allIncludedItems.Remove(item);
                        allArticlesConditionResult.Excluded.Add(item);
                    }
                }
                // Check quantities again:
                PerformQuantityCheck(criterionContainer);
                foreach (var criterionResult in criterionContainer) {
                    if (criterionResult.Included.Count == 0) { continue; }
                    if (criterionResult.Criterion.TypeOperator != PromotionTypeOperator.DifferentFrom) { continue; }
                    criterionResult.Included.Clear();
                    criterionResult.QuantityGroups.Clear();
                }
            } else if (criterionContainer.Count > 1
                       && criterionContainer.Any(c => c.Criterion.TypeOperator == PromotionTypeOperator.EqualTo)
                       && criterionContainer.Any(c => c.Criterion.TypeOperator == PromotionTypeOperator.DifferentFrom)) {
                // filter all included except all excluded
                foreach (var conditionResult in criterionContainer) {
                    if (conditionResult.Criterion.TypeOperator == PromotionTypeOperator.DifferentFrom) { continue; }
                    var allOtherExcludedItems = new HashSet<PromotionArticleData>();
                    foreach (var otherCriterionResult in criterionContainer) {
                        if (ReferenceEquals(conditionResult.Criterion, otherCriterionResult.Criterion) && conditionResult.Criterion.TypeOperator != otherCriterionResult.Criterion.TypeOperator) { continue; }
                        allOtherExcludedItems.AddRange(otherCriterionResult.Excluded);
                    }
                    var remainingIncluded = conditionResult.Included.Except(allOtherExcludedItems).ToSafeHashSet();
                    var included = conditionResult.Included.ToList();
                    foreach (var item in included) {
                        if (remainingIncluded.Contains(item)) { continue; }
                        conditionResult.Included.Remove(item);
                        conditionResult.Excluded.Add(item);
                    }
                }
                criterionContainer = criterionContainer.Where(c => c.Criterion.TypeOperator == PromotionTypeOperator.EqualTo).ToList();
                // Check quantities again:
                PerformQuantityCheck(criterionContainer);
            }

            return criterionContainer;
        }

        public static HashSet<PromotionArticleData> GetValidArticles(List<PromotionConditionResult> conditionResultToProcess) {
            var groupedByCondition = conditionResultToProcess.GroupBy(c => c.Criterion.Group).ToSafeList();
            if (groupedByCondition.Count == 1) {
                return conditionResultToProcess.SelectMany(c => c.Included).ToSafeHashSet();
            }

            var includedPerCondition = groupedByCondition.Select(g => g.SelectMany(c => c.Included).ToSafeList()).ToSafeList();
            if (includedPerCondition.IsEmpty()) {
                return new HashSet<PromotionArticleData>(0);
            }
            return includedPerCondition.Aggregate((l1, l2) => l1.Intersect(l2).ToSafeList()).ToSafeHashSet();
        }

        private static void PerformQuantityCheck(List<PromotionConditionResult> criterionContainer) {
            foreach (var criterionResult in criterionContainer) {
                if (criterionResult.Included.Count == 0) {
                    criterionResult.QuantityGroups = new KeyValueList<HashSet<PromotionArticleData>, int>(0);
                    continue;
                }
                var includedItemSet = criterionResult.Included.ToSafeHashSet();
                var remainingList = QuantityCondition(criterionResult.Criterion, includedItemSet);
                criterionResult.QuantityGroups = remainingList;
                var remaining = new HashSet<PromotionArticleData>();
                criterionResult.Times = 0;
                foreach (var remainingItems in remainingList) {
                    remaining.AddRange(remainingItems.Key);
                    criterionResult.Times += remainingItems.Value;
                }
                foreach (var item in includedItemSet) {
                    if (remaining.Contains(item)) { continue; }
                    criterionResult.Included.Remove(item);
                    criterionResult.AddItem(false, item);
                }
            }
        }

        /// <summary>
        /// Retrieve the <see cref="PromotionConditionWrapper"/> objects, grouped per <see cref="Group"/>.
        /// </summary>
        /// <param name="parameters">Promotion parameters</param>
        /// <param name="hasAllArticlesCriteria">out parameter: informs the caller whether there was a <see cref="PromotionConditionType"/>.<see cref="PromotionConditionType.AllArticles"/> present.</param>
        /// <returns></returns>
        public static List<List<PromotionConditionWrapper>> GetGroupedConditions(PromotionConditionParameters parameters, out bool hasAllArticlesCriteria) {
            hasAllArticlesCriteria = false;
            var temp = new Dictionary<string, List<PromotionConditionWrapper>>();
            foreach (var condition in parameters.Promotion.GetConditions(parameters.Kind)) {
                if (!temp.TryGetValue(condition.Group, out var criteria)) {
                    criteria = new List<PromotionConditionWrapper>();
                    temp.Add(condition.Group, criteria);
                }
                criteria.Add(condition);
                if (condition.ConditionType == PromotionConditionType.AllArticles) {
                    hasAllArticlesCriteria = true;
                }
            }
            var conditions = temp.OrderBy(p => p.Key).Select(p => p.Value).ToList();
            return conditions;
        }

        public static KeyValueList<HashSet<PromotionArticleData>, int> QuantityCondition(PromotionConditionWrapper criterion, HashSet<PromotionArticleData> items) {
            var temp = new KeyValueList<HashSet<PromotionArticleData>, int>();
            if (criterion.IsIndividual) {
                foreach (var grouping in items.GroupBy(i => i.ArticleID)) {
                    var filteredGrouping = PerformGroupingFilter(criterion, grouping.ToSafeHashSet(), out var tempTimes);
                    if (filteredGrouping.IsEmpty()) { continue; }
                    temp.Add(filteredGrouping, tempTimes);
                }
                return temp;
            }
            var filteredGroupGrouping = PerformGroupingFilter(criterion, items, out var times);
            if (filteredGroupGrouping.HasAny()) {
                temp.Add(filteredGroupGrouping, times);
            }
            return temp;
        }

        private static HashSet<PromotionArticleData> PerformGroupingFilter(PromotionConditionWrapper criterion, HashSet<PromotionArticleData> items, out int times) {
            var valueCalcFunc = GetValueCalculationFunction(criterion);
            var valueToCheck = items.Sum(valueCalcFunc);

            // Note for Per grouping: Can't filter items here, since you don't know the action to perform yet. 
            // If there are an assortment of articles to pick from and the action handles a specific order, you can't decide here what the action will do.
            if (!QuantityCheck(criterion, valueToCheck, out times)) {
                return new HashSet<PromotionArticleData>();
            }
            return items;
        }

        private static Func<PromotionArticleData, decimal> GetValueCalculationFunction(PromotionConditionWrapper criterion) {
            Func<PromotionArticleData, decimal> valueCalcFunc;
            switch (criterion.ConditionType) {
                case PromotionConditionType.DocumentValue:
                case PromotionConditionType.DocumentValueExcludingOtherPromotion:
                case PromotionConditionType.ProductValue:
                case PromotionConditionType.BrandValue:
                    valueCalcFunc = i => i.Quantity * i.Price;
                    break;
                case PromotionConditionType.Weight:
                    valueCalcFunc = i => i.Quantity * i.Weight;
                    break;
                default:
                    valueCalcFunc = i => i.Quantity;
                    break;
            }
            return valueCalcFunc;
        }

        private static bool QuantityCheck(PromotionConditionWrapper criterion, decimal quantity, out int times) {
            bool checkResult;
            times = 1;
            var criteriaAmount = criterion.PerAmount.To<decimal>();
            switch (criterion.QuantityComparisonOperator) {
                case PromotionQuantityComparisonOperator.SmallerThan:
                    checkResult = quantity < criteriaAmount;
                    break;
                case PromotionQuantityComparisonOperator.DifferentFrom:
                    checkResult = quantity != criteriaAmount;
                    break;
                //case PromotionOperator.GreaterOrEqual:
                default:
                    if (criterion.Grouping == PromotionGrouping.Per) {
                        var tempTimes = 0;
                        if (quantity >= criteriaAmount) {
                            tempTimes = decimal.Floor(decimal.Divide(quantity, criteriaAmount)).To<int>();
                        }
                        times = tempTimes;
                        return tempTimes > 0;
                    }
                    checkResult = quantity >= criteriaAmount;
                    break;
            }
            times = checkResult ? times : 0;
            return checkResult;
        }
    }
}