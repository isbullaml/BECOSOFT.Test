using BECOSOFT.Utilities.Collections;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Extensions.Numeric;
using BECOSOFT.Utilities.Models.Prices;
using BECOSOFT.Utilities.Models.Promotions;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BECOSOFT.Utilities.Promotions {
    public abstract class BasePromotionCalculator {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private static readonly ILogger _nullLogger = LogManager.CreateNullLogger();

        /// <summary>
        /// Returns a null logger if <see cref="PromotionParameters"/>.<see cref="PromotionParameters.EnableProcessLogging"/> is enabled.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected static ILogger GetLogger(BasePromotionParameters parameters) => parameters.EnableProcessLogging ? _logger : _nullLogger;

        /// <summary>
        /// Minimum group level (usually 1)
        /// </summary>
        protected abstract int MinimumGroupLevel { get; }

        /// <summary>
        /// Maximum group level (usually 5), should be matched to the actual maximum group levels
        /// </summary>
        protected abstract int MaximumGroupLevel { get; }

        /// <summary>
        /// This method should fill a <see cref="PromotionSettings"/> object.
        /// </summary>
        /// <returns></returns>
        protected abstract PromotionSettings FillSettings();

        /// <summary>
        /// This method should return a <see cref="List{T}"/> of <see cref="PromotionWrapper"/>-objects that are valid based on the given <see cref="PromotionParameters"/>.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected abstract List<PromotionWrapper> GetValidPromotions(BasePromotionParameters parameters);

        /// <summary>
        /// This method should return a <see cref="PriceContainer"/> with <see cref="PromotionParameters.PriceTypeID"/> that will be used if <see cref="PromotionParameters.ChooseBestPromotion"/> on <see cref="PromotionParameters"/> is <see langword="true"/>
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="articleIDs">ArticleIDs for which prices should be retrieved</param>
        /// <returns></returns>
        protected abstract PriceContainer GetArticlePrices(BasePromotionParameters parameters, List<int> articleIDs);


        public MultiPromotionProcessResult Process(MultiPromotionParameters parameters) {
            var articleDataContainer = parameters.PromotionArticleContainer ?? new PromotionArticleContainer();
            parameters.PromotionArticleContainer = articleDataContainer;
            var parameterList = parameters.ToParameters();
            var logger = GetLogger(parameters);
            var processResult = new MultiPromotionProcessResult();
            if (parameterList.IsEmpty()) {
                logger.Info("Exiting calculation: invalid parameters");
                return processResult;
            }
            var settings = FillSettings();
            if (parameters.EnableProcessLogging) {
                logger.Info("Promotion settings: ");
                logger.Info("{@0}", settings);
            }

            if (!CanProcessParameters(parameters, settings)) {
                logger.Info("Exiting calculation: cannot process parameters");
                return processResult;
            }
            // Retrieve promotions
            var promotions = GetValidPromotions(parameters);
            var filteredArticleData = Filter(parameterList, settings);
            // Get article info
            // Also check if the PromotionConditionType Tag is present in one of the promotions that was found. If tag condition is present, tag data is retrieved.
            parameters.PromotionArticleContainer = UpdatePromotionArticleData(parameterList[0], promotions, filteredArticleData.AlwaysExecuteArticles);
            foreach (var paramObj in parameterList) {
                var result = ProcessInternal(paramObj, promotions);
                processResult.Results.Add(result);
            }
            return processResult;
        }

        public PromotionProcessResult Process(PromotionParameters parameters) {
            var logger = GetLogger(parameters);
            var settings = FillSettings();
            if (parameters.EnableProcessLogging) {
                logger.Info("Promotion settings: ");
                logger.Info("{@0}", settings);
            }

            if (!CanProcessParameters(parameters, settings)) {
                var processResult = new PromotionProcessResult(parameters) {
                    DidProcessPromotions = false
                };
                logger.Info("Exiting calculation");
                return processResult;
            }
            // Retrieve promotions
            var promotions = GetValidPromotions(parameters);
            return ProcessInternal(parameters, promotions);
        }

        private PromotionProcessResult ProcessInternal(PromotionParameters parameters, List<PromotionWrapper> promotions) {
            var logger = GetLogger(parameters);
            var settings = FillSettings();

            var processResult = new PromotionProcessResult(parameters);
            if (promotions.IsEmpty()) {
                logger.Debug("Exiting calculation");
                processResult.DidProcessPromotions = false;
                return processResult;
            }

            var filteredArticleData = Filter(parameters, settings);
            var activatedPromotionIDs = parameters.ActivatedPromotionIDs.ToSafeHashSet();

            var potentialArticleIDAdded = promotions.SelectMany(p => p.Actions.Where(a => a.Type.IsAddArticle()).Select(a => a.GetValue<int>(1))).Distinct().ToList();
            var updatedArticlePrices = GetArticlePricesInternal(parameters, potentialArticleIDAdded);

            // Get article info
            // Also check if the PromotionConditionType Tag is present in one of the promotions that was found. If tag condition is present, tag data is retrieved.
            var promotionArticleContainer = UpdatePromotionArticleData(parameters, promotions, filteredArticleData.AlwaysExecuteArticles);

            var originalData = filteredArticleData.FilteredArticles.Select(a => a.Clone()).ToList();
            var originalDataAlwaysExecute = filteredArticleData.AlwaysExecuteArticles.Select(a => a.Clone()).ToList();
            var remainingArticleData = originalData.ToList();
            var lastPromotion = promotions[promotions.Count - 1];
            var lastPromotionHasDocumentValue = lastPromotion.HasCondition(PromotionConditionKind.All, PromotionConditionType.DocumentValue)
                                                || !lastPromotion.HasAction(PromotionActionType.ArticleTypePrice,
                                                                           PromotionActionType.ChangeNetPrice,
                                                                           PromotionActionType.Discount,
                                                                           PromotionActionType.ProgressiveDiscount);
            foreach (var promotion in promotions) {
                logger.Info("Check {0} - '{1}'", promotion.PromotionID, promotion.Name);
                if (promotion.Actions.IsEmpty() || promotion.PromotionConditions.IsEmpty()) {
                    logger.Warn("Promotion has no actions or conditions, skipping");
                    continue;
                }
                var promotionProcessInfo = new PromotionProcessInfo {
                    Promotion = promotion,
                    Settings = settings,
                    Parameters = parameters,
                };
                if (promotion.HasAction(PromotionActionType.Voucher)) {
                    if (!CanProcessActionVoucher(promotionProcessInfo)) {
                        continue;
                    }
                }
                if (promotion.AlwaysExecute && !CanProcessAlwaysExecute(promotion, promotionProcessInfo)) {
                    continue;
                }
                if (parameters.DocumentTypeID != 0 && promotion.ExcludedDocuments.HasAny()) {
                    if (promotion.ExcludedDocuments.Any(edt => edt.DocumentTypeID == parameters.DocumentTypeID)) {
                        logger.Warn("Promotion cannot be checked because {0} ({1}) contains {2}.",
                                    nameof(promotion.ExcludedDocuments), string.Join(",", promotion.ExcludedDocuments.Select(edt => edt.DocumentTypeID)), parameters.DocumentTypeID);
                        continue; // Normally these should be filtered in the step "GetValidPromotions(parameters)", but this check is a failsafe
                    }
                }
                List<ArticleData> previousRemainingData = null;
                if (promotion.AlwaysExecute || (lastPromotionHasDocumentValue && promotion.HasCondition(PromotionConditionKind.All, PromotionConditionType.DocumentValue))) {
                    previousRemainingData = remainingArticleData;
                    remainingArticleData = promotion.AlwaysExecute ? originalDataAlwaysExecute.ToList() : originalData.ToList();
                    logger.Info("resetting remaining article data because the last promotion has a document value condition");
                }
                logger.Info("{0} remaining articles", remainingArticleData.Count);
                var indices = remainingArticleData.Select(a => a.Index).ToList();
                var items = GetPromotionArticleData(remainingArticleData, promotionArticleContainer);

                // check activation conditions if the promotion is not present in the already activated promotion ids list
                if (!activatedPromotionIDs.Contains(promotion.PromotionID)) {
                    var activationConditionParameters = GetConditionParameters(promotion, items, PromotionConditionKind.Activation);
                    var activationConditionResult = ConditionChecker.CheckConditions(activationConditionParameters);
                    if (promotion.IsActionVoucher && activationConditionResult == null) {
                        logger.Warn("Skipping promotion because it is an action voucher promotion without '{0}' conditions.", PromotionConditionKind.Activation);
                        continue;
                    }
                    if (activationConditionResult != null) {
                        var numberOfTimes = PromotionCalculationHelpers.CalculateNumberOfTimes(activationConditionResult).GetValueOrDefault(1);
                        var activatedPromotion = GetActivatedPromotionResult(promotion, numberOfTimes, settings);
                        logger.Warn("Promotion is added to {0} for {1} number of times.", nameof(parameters.ActivatedPromotionIDs), numberOfTimes);
                        processResult.ActivatedPromotions.Add(activatedPromotion);
                        // no need to mark articles as "used"
                        continue;
                    }
                } else {
                    logger.Warn("Promotion is already present in {0}.", nameof(parameters.ActivatedPromotionIDs));
                }

                // Check default conditions
                var conditionParameters = GetConditionParameters(promotion, items, PromotionConditionKind.Default);
                var hasConditionSpecificActions = HasConditionSpecificActions(promotion, conditionParameters);
                var conditionResult = ConditionChecker.CheckConditions(conditionParameters);
                if (conditionResult.IsEmpty() || conditionResult.All(cr => cr.QuantityGroups.IsEmpty())) {
                    logger.Info("No conditions met");
                    continue;
                }
                if (activatedPromotionIDs.Contains(promotion.PromotionID)) {
                    processResult.ActivatedPromotionIDsInResult.Add(promotion.PromotionID);
                }
                // Check action conditions
                var actionConditionParameters = GetConditionParameters(promotion, items, PromotionConditionKind.Action);
                var actionConditionResult = ConditionChecker.CheckConditions(actionConditionParameters);

                if (lastPromotionHasDocumentValue && previousRemainingData != null) {
                    remainingArticleData = previousRemainingData;
                    indices = remainingArticleData.Select(a => a.Index).ToList();
                }

                // Handle actions
                var combinedConditions = new PromotionCombinedConditionResult {
                    DefaultConditionResult = conditionResult,
                    ActionConditionResult = actionConditionResult,
                    HasConditionSpecificActions = hasConditionSpecificActions,
                };
                var actionResult = PerformHandleActions(promotionProcessInfo, remainingArticleData, combinedConditions);

                if (CanChooseBestPromotion(promotionProcessInfo) && !IsPromotionBeneficial(updatedArticlePrices, actionResult, parameters)) {
                    logger.Info("Promotion worse than prices of the eligible articles.");
                    continue;
                }

                ProcessActionResult(processResult, actionResult, indices, promotionProcessInfo);
                var promotionInfo = promotion.ToPromotionInfo();
                processResult.PromotionNames.Add(promotionInfo);
                // Filter remaining articles
                remainingArticleData = GetRemainingArticleData(remainingArticleData, actionResult.ArticleResultPerIndex, promotionProcessInfo);
            }
            // add missing articles from the original article data to the article per index result
            var articleData = parameters.Data;
            var indexedArticleData = articleData.ToDictionary(a => a.Index);
            foreach (var item in indexedArticleData) {
                if (processResult.ArticleResultPerIndex.ContainsKey(item.Key)) { continue; }
                processResult.ArticleResultPerIndex.Add(item.Key, new PromotionArticleProcessResult(item.Value));
            }
            ReduceArticleResultPerIndex(processResult);
            UpdateResult(processResult);
            logger.Info("Finished processing");
            return processResult;
        }

        private static bool HasConditionSpecificActions(PromotionWrapper promotion, PromotionConditionParameters conditionParameters) {
            var conditionGroups = ConditionChecker.GetGroupedConditions(conditionParameters, out _);
            var conditionGroupsSet = conditionGroups.Select(c => c.First().Group).ToHashSet();
            var hasConditionSpecificActions = promotion.Actions.All(a => a.ConditionGroup.HasNonWhiteSpaceValue() && conditionGroupsSet.Contains(a.ConditionGroup));
            return hasConditionSpecificActions;
        }

        private bool CanProcessAlwaysExecute(PromotionWrapper promotion, PromotionProcessInfo promotionProcessInfo) {
            if (!promotion.AlwaysExecute) {
                return false;
            }
            var allowedActions = new List<PromotionActionType> {
                PromotionActionType.AddArticleWithPrice,
                PromotionActionType.AddArticleWithDiscount,
                PromotionActionType.ActivatePromotion,
                PromotionActionType.Voucher,
            };
            var promotionActions = promotion.Actions.Select(a => a.Type).ToDistinctList();
            var remainingActions = promotionActions.Except(allowedActions).ToList();
            if (remainingActions.HasAny()) {
                var logger = GetLogger(promotionProcessInfo.Parameters);
                logger.Warn("Cannot process '{0}' promotion because it has invalid action types ({1}). Allowed actions for '{0}' are: {2}",
                            nameof(promotion.AlwaysExecute), string.Join(", ", remainingActions), string.Join(", ", allowedActions));

                return false;
            }
            return true;
        }

        protected PriceContainer GetArticlePricesInternal(PromotionParameters parameters, List<int> articleIDsToFetchPricesFor) {
            if (parameters.ArticlePrices == null) {
                parameters.SetPriceContainer(new PriceContainer(parameters.PriceTypeID, parameters.PriceLogic));
            }
            var logger = GetLogger(parameters);
            var container = parameters.ArticlePrices;
            if (!parameters.ChooseBestPromotion) {
                logger.Info("Skipping sale price fetching because {0} is false.", nameof(parameters.ChooseBestPromotion));
                return container;
            }
            var articleIDsNotPresent = parameters.Data.Select(a => a.ArticleID).Union(articleIDsToFetchPricesFor)
                                                 .Except(container?.ArticleIDsWithPrices ?? new List<int>(0))
                                                 .ToDistinctList();
            if (articleIDsNotPresent.IsEmpty()) {
                logger.Info("No new sale prices to fetch.");
                return container;
            }
            logger.Info("Fetching sale prices for {0}.", string.Join(",", articleIDsNotPresent));
            return GetArticlePrices(parameters, articleIDsToFetchPricesFor);
        }

        private PromotionArticleContainer UpdatePromotionArticleData(PromotionParameters parameters, List<PromotionWrapper> promotions, List<ArticleData> filteredArticleData) {
            var logger = GetLogger(parameters);
            var hasTagsCondition = promotions.Any(p => p.PromotionConditions.Any(pc => pc.IsTagCondition()));
            if (parameters.PromotionArticleContainer == null) {
                parameters.PromotionArticleContainer = new PromotionArticleContainer();
            }
            var existingContainer = parameters.PromotionArticleContainer;
            // if the current container has no tag info, and there are promotions with tags, fetch them:
            var existingArticleIDs = !existingContainer.HasTagInfo && hasTagsCondition ? new List<int>() : existingContainer.ArticleIDs;
            var articleIDsToFetch = filteredArticleData.Select(ad => ad.ArticleID).Except(existingArticleIDs).Distinct().ToList();
            Dictionary<int, PromotionArticleWrapper> promotionArticleData = null;
            if (articleIDsToFetch.HasAny()) {
                logger.Info("Fetching article data for {0} with includeTags value '{1}'.", string.Join(",", articleIDsToFetch), existingContainer.HasTagInfo || hasTagsCondition);
                promotionArticleData = GetNewArticleData(articleIDsToFetch, existingContainer.HasTagInfo || hasTagsCondition);
            }
            existingContainer.UpdateData(promotionArticleData, existingContainer.HasTagInfo || hasTagsCondition);
            return existingContainer;
        }

        protected abstract Dictionary<int, PromotionArticleWrapper> GetNewArticleData(List<int> articleIDs, bool includeTags);

        private static decimal GetPrice(ArticleData articleData, PriceContainer prices, bool basePrice, decimal? quantity = null) {
            var q = quantity ?? articleData.Quantity;
            var calculation = prices.GetPriceCalculation(articleData.ArticleID, q);
            if (calculation.IsEmpty) {
                return q * articleData.BasePrice;
            }
            return q * (basePrice ? calculation.BasePriceWithoutPromotion : calculation.PriceWithDiscount);
        }

        private static decimal GetPrice(PromotionAddArticle addAction, PriceContainer prices) {
            var calculation = prices.GetPriceCalculation(addAction.ArticleID, addAction.Quantity);
            return calculation.BasePriceWithoutPromotion;
        }

        private bool IsPromotionBeneficial(PriceContainer priceContainer, PromotionProcessResult actionResult, PromotionParameters parameters) {
            var logger = GetLogger(parameters);
            var preCalculationValue = 0m;
            var newDataValue = 0m;
            var adjustmentValue = 0m;
            var markedValue = 0m;
            foreach (var grouping in actionResult.ArticleResultPerIndex.Values.GroupBy(a => a.ArticleID)) {
                foreach (var articleResult in grouping) {
                    if (articleResult.IsUntouched) { continue; }
                    preCalculationValue += GetPrice(articleResult.OriginalArticleData, priceContainer, false);
                    if (articleResult.NewArticleData != null) {
                        newDataValue += GetPrice(articleResult.NewArticleData, priceContainer, false);
                    }
                    foreach (var adjustment in articleResult.ArticleDataAdjustments) {
                        if (adjustment.IsMarkedArticle) {
                            markedValue += GetPrice(articleResult.OriginalArticleData, priceContainer, true, adjustment.Quantity);
                        } else {
                            var adjustedData = articleResult.PerformAdjustment(adjustment);
                            adjustmentValue += adjustedData.Quantity * adjustedData.Price;
                        }
                    }
                }
            }
            var addedArticlesValue = actionResult.ArticlesWithPrice.Sum(a => a.Quantity * Math.Abs(a.Price));
            var addedArticlesWithDiscountValue = actionResult.ArticlesWithDiscount.Sum(a => a.Quantity * GetPrice(a, priceContainer) * a.Discount / 100m);
            var discountVoucherValue = actionResult.VoucherWithDiscountValue.Sum(a => a.DiscountValue);
            var discountVoucherCalculatedValue = actionResult.VoucherWithDiscount.Sum(a => a.CalculatedDiscountValue);
            var totalValue = newDataValue + adjustmentValue + markedValue;
            var actionValue = addedArticlesValue + addedArticlesWithDiscountValue + discountVoucherValue + discountVoucherCalculatedValue;

            var result = totalValue - actionValue <= preCalculationValue;
            if (parameters.EnableProcessLogging) {
                logger.Info("Result: is beneficial? {3} beneficial: {0:F4} total - {1:F4} action value <= {2:F4} precalculation value", totalValue, actionValue, preCalculationValue, result);
            }
            return result;
        }

        /// <summary>
        /// Check <see cref="PromotionProcessResult"/> for articles with price or discount already present (to avoid unnecessary article removal and addition).
        /// </summary>
        /// <param name="result"></param>
        private static void UpdateResult(PromotionProcessResult result) {
            var originalData = result.OriginalParameters.Data;
            var articleDataAddedByPromotion = originalData.Where(a => a.IsAddedByPromotion)
                                                          .GroupBy(a => Tuple.Create(a.PromotionID, a.ArticleID))
                                                          .ToDictionary(a => a.Key, a => a.ToList());
            UpdateActions(articleDataAddedByPromotion, result.ArticlesWithPrice, result, (article, action) => Math.Abs(article.Price) == Math.Abs(action.Price));
            UpdateActions(articleDataAddedByPromotion, result.ArticlesWithDiscount, result, (article, action) => article.Discount1 == action.Discount);
            foreach (var article in articleDataAddedByPromotion) {
                if (!result.ArticlesWithPrice.Any(a => a.PromotionID == article.Key.Item1 && a.ArticleID == article.Key.Item2)
                    && !result.ArticlesWithDiscount.Any(a => a.PromotionID == article.Key.Item1 && a.ArticleID == article.Key.Item2)) {
                    result.ArticlesToRemove.AddRange(article.Value);
                }
            }
        }

        /// <summary>
        /// Merge the result from <see cref="HandleActions"/> with the existing <see cref="PromotionProcessResult"/> object.
        /// </summary>
        /// <param name="promotionResult">Global promotion result</param>
        /// <param name="actionResult">Result from the <see cref="HandleActions"/> function.</param>
        /// <param name="indices">List of indices that were processed</param>
        /// <param name="promotionProcessInfo"></param>
        private static void ProcessActionResult(PromotionProcessResult promotionResult, PromotionProcessResult actionResult,
                                                List<int> indices, PromotionProcessInfo promotionProcessInfo) {
            var promotion = promotionProcessInfo.Promotion;
            var parameters = promotionProcessInfo.Parameters;
            var logger = GetLogger(parameters);
            logger.Info("Adding action result to {0}.", nameof(promotionResult.ArticleResultPerIndex));
            var disablePromotionTagging = promotion.DisablePromotionTagging || promotion.AlwaysExecute;
            if (promotionResult.ArticleResultPerIndex.IsEmpty()) {
                logger.Info("{0} is empty.", nameof(promotionResult.ArticleResultPerIndex));
                if (disablePromotionTagging) {
                    foreach (var articleProcessResult in actionResult.ArticleResultPerIndex) {
                        var artData = articleProcessResult.Value;
                        if (articleProcessResult.Value.NewArticleData == null && articleProcessResult.Value.ArticleDataAdjustments.All(a => a.IsMarkedArticle)) {
                            logger.Info("{0} at index {1}: remains because it is a fully marked article and {2} or {3} is enabled on the promotion.", artData.ArticleID, artData.Index,
                                        nameof(promotion.DisablePromotionTagging), nameof(promotion.AlwaysExecute));
                            continue;
                        }
                        logger.Info("{0} at index {1}: added.", artData.ArticleID, artData.Index);
                        promotionResult.ArticleResultPerIndex.Add(articleProcessResult.Key, articleProcessResult.Value);
                    }
                } else {
                    if (parameters.EnableProcessLogging) {
                        foreach (var articleProcessResult in promotionResult.ArticleResultPerIndex) {
                            var artData = articleProcessResult.Value;
                            logger.Info("{0} at index {1}: added.", artData.ArticleID, artData.Index);
                        }
                    }
                    promotionResult.ArticleResultPerIndex.AddRange(actionResult.ArticleResultPerIndex);
                }
            } else {
                var actionResultDict = actionResult.ArticleResultPerIndex;
                var actionResultsDict = promotionResult.ArticleResultPerIndex;
                foreach (var index in indices) {
                    var newResult = actionResultDict.TryGetValueWithDefault(index);
                    if (newResult == null) { continue; }
                    var existingResult = actionResultsDict.TryGetValueWithDefault(index);
                    if (existingResult == null) {
                        logger.Info("{0} at index {1}: added.", newResult.ArticleID, index);
                        promotionResult.ArticleResultPerIndex.Add(index, newResult);
                        continue;
                    }
                    if (newResult.ArticleDataAdjustments.IsEmpty() && newResult.NewArticleData == null) {
                        logger.Info("{0} at index {1}: skipped because no adjustments or new article data present", newResult.ArticleID, index);
                        continue;
                    }
                    if (disablePromotionTagging && newResult.ArticleDataAdjustments.All(a => a.IsMarkedArticle)) {
                        logger.Info("{0} at index {1}: remains because it is a fully marked article and {2} or {3} is enabled on the promotion.", newResult.ArticleID, index,
                                    nameof(promotion.DisablePromotionTagging), nameof(promotion.AlwaysExecute));
                        continue;
                    }
                    logger.Info("{0} at index {1}: added {2} adjustments", newResult.ArticleID, index, newResult.ArticleDataAdjustments.Count);
                    existingResult.ArticleDataAdjustments.AddRange(newResult.ArticleDataAdjustments);
                    existingResult.NewArticleData = newResult.NewArticleData;
                }
            }
            promotionResult.ArticlesWithPrice.AddRange(actionResult.ArticlesWithPrice);
            promotionResult.ArticlesWithDiscount.AddRange(actionResult.ArticlesWithDiscount);
            promotionResult.VoucherWithDiscount.AddRange(actionResult.VoucherWithDiscount);
            promotionResult.VoucherWithDiscountValue.AddRange(actionResult.VoucherWithDiscountValue);
        }

        /// <summary>
        /// Reduce the number of AdjustedArticleData items by grouping equal items and summing the quantity
        /// </summary>
        /// <param name="promotionResult"><see cref="PromotionProcessResult"/> to reduce.</param>
        private static void ReduceArticleResultPerIndex(PromotionProcessResult promotionResult) {
            foreach (var articleResult in promotionResult.ArticleResultPerIndex) {
                var adjusted = articleResult.Value.ArticleDataAdjustments;
                if (adjusted.Count <= 1) { continue; }
                var reducedAdjusted = new List<ArticleDataAdjustment>();
                foreach (var grouping in adjusted.GroupBy(a => a)) {
                    reducedAdjusted.Add(grouping.Key);
                    grouping.Key.Quantity = grouping.Sum(a => a.Quantity);
                }
                articleResult.Value.ArticleDataAdjustments = reducedAdjusted;
            }
        }

        private static List<ArticleData> GetRemainingArticleData(List<ArticleData> articleData,
                                                                 Dictionary<int, PromotionArticleProcessResult> actionResult,
                                                                 PromotionProcessInfo promotionProcessInfo) {
            var promotion = promotionProcessInfo.Promotion;
            var parameters = promotionProcessInfo.Parameters;
            var logger = GetLogger(parameters);
            var disablePromotionTagging = promotion.DisablePromotionTagging || promotion.AlwaysExecute;
            var remainingArticleData = new List<ArticleData>();
            var articleDataPerIndex = articleData.GroupBy(a => a.Index).ToDictionary(a => a.Key, a => a.ToList());
            foreach (var indexData in articleDataPerIndex) {
                var artData = actionResult.TryGetValueWithDefault(indexData.Key);
                if (artData == null) {
                    // if the index is not present in the action result, add all articles for that index
                    remainingArticleData.AddRange(indexData.Value);
                    logger.Info("{0} at index {1} remains fully eligible.", indexData.Value[0].ArticleID, indexData.Key);
                    continue;
                }
                var hasAdjustments = artData.ArticleDataAdjustments.HasAny();
                if (artData.NewArticleData == null) {
                    if (hasAdjustments) {
                        // article is fully handled by the promotion
                        if (disablePromotionTagging && artData.ArticleDataAdjustments.All(a => a.IsMarkedArticle)) {
                            remainingArticleData.Add(artData.OriginalArticleData);
                            logger.Info("{0} at index {1} remains because it is a fully marked article and {2} or {3} is enabled on the promotion.", artData.ArticleID, artData.Index,
                                        nameof(promotion.DisablePromotionTagging), nameof(promotion.AlwaysExecute));
                        }
                        continue;
                    }
                    // article not handled by the promotion
                    remainingArticleData.Add(artData.OriginalArticleData);
                    logger.Info("{0} at index {1} remains fully eligible ({2}).", artData.ArticleID, artData.Index, artData.OriginalArticleData.Quantity);
                    continue;
                }
                // article is partially handled by the promotion
                remainingArticleData.Add(artData.NewArticleData);
                logger.Info("{0} at index {1} remains partially eligible ({2}/{3}).", artData.ArticleID, artData.Index, artData.NewArticleData.Quantity, artData.OriginalArticleData.Quantity);
            }
            logger.Info("{0} remaining articles after processing", remainingArticleData.Count);
            return remainingArticleData;
        }

        private static PromotionProcessResult PerformHandleActions(PromotionProcessInfo promotionProcessInfo,
                                                                   List<ArticleData> articleData,
                                                                   PromotionCombinedConditionResult conditionResult) {
            if (!conditionResult.HasConditionSpecificActions) {
                return HandleActions(promotionProcessInfo, articleData, conditionResult);
            }
            var logger = GetLogger(promotionProcessInfo.Parameters);
            var promotion = promotionProcessInfo.Promotion.Copy();
            PromotionProcessResult result = null;
            var (_, numberOfTimes) = GetConditionResultToProcess(conditionResult, promotionProcessInfo.Parameters);
            var conditionGroupings = promotion.Actions.GroupBy(a => a.ConditionGroup).ToList();
            logger.Info("Will process promotion actions per condition group");
            foreach (var conditionGrouping in conditionGroupings) {
                var newPromotion = promotion.Copy();
                newPromotion.Actions.Clear();
                var actions = conditionGrouping.ToList();
                newPromotion.Actions.AddRange(actions);
                var newConditionResult = new PromotionCombinedConditionResult {
                    DefaultConditionResult = conditionResult.DefaultConditionResult.Where(cr => cr.Criterion.Group == conditionGrouping.Key).ToList(),
                    ActionConditionResult = conditionResult.ActionConditionResult?.Where(cr => cr.Criterion.Group == conditionGrouping.Key).ToList(),
                    OverrulingNumberOfTimes = numberOfTimes,
                };
                if (newConditionResult.DefaultConditionResult.IsEmpty() || (newConditionResult.ActionConditionResult != null && newConditionResult.ActionConditionResult.IsEmpty())) {
                    continue;
                }
                var newPromotionProcessInfo = new PromotionProcessInfo {
                    Parameters = promotionProcessInfo.Parameters,
                    Settings = promotionProcessInfo.Settings,
                    Promotion = newPromotion,
                };
                var tempResult = HandleActions(newPromotionProcessInfo, articleData, newConditionResult);
                if (result == null) {
                    result = tempResult;
                } else {
                    result.Merge(tempResult);
                }
            }
            return result;
        }

        private static PromotionProcessResult HandleActions(PromotionProcessInfo promotionProcessInfo,
                                                            List<ArticleData> articleData,
                                                            PromotionCombinedConditionResult conditionResult) {
            var promotion = promotionProcessInfo.Promotion;
            var settings = promotionProcessInfo.Settings;
            var promotionParameters = promotionProcessInfo.Parameters;
            var logger = GetLogger(promotionParameters);
            var (conditionResultToProcess, numberOfTimes) = GetConditionResultToProcess(conditionResult, promotionParameters);

            var indexedArticleData = articleData.ToDictionary(a => a.Index);
            var result = HandleActionsWithSortingPriority(promotionProcessInfo, conditionResultToProcess, numberOfTimes, indexedArticleData);
            if (result != null) {
                MarkArticlesInDefaultConditions(conditionResult, numberOfTimes, indexedArticleData, result, promotionProcessInfo, logger);
                return result;
            }
            result = new PromotionProcessResult();
            var promotionEligibleArticlesParameters = new PromotionEligibleArticlesParameters(conditionResultToProcess, numberOfTimes, promotionParameters.EnablePromotionGroupingIndex) {
                ActionArticleType = 0,
                Quantity = int.MaxValue,
            };
            var eligibleArticles = PromotionEligibleArticleChecker.GetEligibleArticles(promotionEligibleArticlesParameters);
            var allIncluded = eligibleArticles.Included.SelectMany(c => c).ToList();
            if (promotion.Actions.All(a => a.Type == PromotionActionType.Voucher && a.GetValue<decimal>(1).RoundTo(2) != 0)) {
                // if the action is a voucher with a discount percentage, check if the eligible articles are eligible for calculation in the discount value
                var temp = new List<EligiblePromotionArticle>();
                foreach (var eligibleArticle in allIncluded) {
                    if (IsEligibleForDiscountValueCalculation(promotionProcessInfo, indexedArticleData, eligibleArticle.ArticleData.Index)) {
                        temp.Add(eligibleArticle);
                    }
                }
                allIncluded = temp;
            }
            var includedArticles = allIncluded.GroupBy(ad => ad.ArticleData.ID).Select(ad => ad.First()).ToList();
            var includedByIndex = includedArticles.GroupBy(pda => pda.ArticleData.Index).ToDictionary(g => g.Key, g => g.ToList());
            var markedArticlesByIndex = eligibleArticles.ToMark.GroupBy(pda => pda.ArticleData.Index).ToDictionary(g => g.Key, g => g.ToList());

            var indices = articleData.Select(a => a.Index).OrderBy(i => i).ToList();

            foreach (var index in indices) {
                var originalArticle = indexedArticleData.TryGetValueWithDefault(index);
                if (originalArticle.VoucherType != PromotionVoucherType.None) {
                    logger.Info("{0} at index {1}: Skipping action because voucher type is {2}.", originalArticle.ArticleID, index, originalArticle.VoucherType);
                    continue;
                }
                var actionResult = result.ArticleResultPerIndex.TryGetValueWithDefaultFunc(index, () => new PromotionArticleProcessResult(originalArticle));
                var included = includedByIndex.TryGetValueWithDefault(index);
                if (included != null) {
                    var includedQuantity = included.Sum(a => a.ArticleData.Quantity);
                    var excludedQuantity = originalArticle.Quantity - includedQuantity;

                    var adjustedArticle = new ArticleDataAdjustment {
                        Quantity = includedQuantity,
                        PromotionID = promotion.PromotionID,
                    };
                    foreach (var action in promotion.Actions) {
                        switch (action.Type) {
                            case PromotionActionType.Discount:
                                adjustedArticle = action.ToAdjustment(includedQuantity, promotion.PromotionID);
                                if (promotion.NotBelowPurchasePrice && (originalArticle.PurchasePrice ?? 0) > 0) {
                                    var temp = actionResult.PerformAdjustment(adjustedArticle);
                                    if (temp.Price < originalArticle.PurchasePrice.GetValueOrDefault()) {
                                        if (adjustedArticle.BasePrice.HasValue) {
                                            adjustedArticle.BasePrice = originalArticle.PurchasePrice.Value;
                                        } else if (adjustedArticle.DiscountAmount.HasValue) {
                                            adjustedArticle.DiscountAmount = temp.BasePrice - originalArticle.PurchasePrice.Value;
                                        } else {
                                            for (var i = 1; i <= 5; i++) {
                                                adjustedArticle.SetDiscount(i, 0);
                                            }
                                            var discount = (1 - (originalArticle.PurchasePrice.Value / temp.BasePrice)) * 100m;
                                            adjustedArticle.SetDiscount(1, discount.RoundTo(2));
                                        }
                                        logger.Info("{0} at index {1}: Overruled action to stay above purchase price ({2}).", originalArticle.ArticleID, index, originalArticle.PurchasePrice);
                                    }
                                }
                                if (adjustedArticle.HasAdjustment) {
                                    logger.Info("{0} at index {1}: {2}", originalArticle.ArticleID, index, adjustedArticle.GetAdjustmentString());
                                }
                                break;
                            case PromotionActionType.ChangeNetPrice:
                                if (originalArticle.ArticleID == action.GetValue<int>(1)) {
                                    adjustedArticle = action.ToAdjustment(includedQuantity, promotion.PromotionID);
                                    logger.Info("{0} at index {1}: Set base price to {2}.", originalArticle.ArticleID, index, adjustedArticle.BasePrice);
                                }
                                break;
                            case PromotionActionType.ChangeNetPriceNotInPromo:
                                // not supported, use action conditions
                                logger.Warn("{0} is not supported, use action conditions instead.", nameof(PromotionActionType.ChangeNetPriceNotInPromo));
                                break;
                            case PromotionActionType.PointFactor:
                                adjustedArticle = action.ToAdjustment(includedQuantity, promotion.PromotionID);
                                logger.Info("{0} at index {1}: Set point factor to {2}.", originalArticle.ArticleID, index, adjustedArticle.PointFactor);
                                break;
                            // all other ActionTypes are handled elsewhere
                        }
                    }
                    if (promotion.AlwaysExecute && !adjustedArticle.HasAdjustment) {
                        adjustedArticle.IsMarkedWithAlwaysExecutePromotion = true;
                    }
                    actionResult.ArticleDataAdjustments.Add(adjustedArticle);
                    logger.Info("Added adjustments to {0} at index {1} for a quantity of {2}", originalArticle.ArticleID, index, includedQuantity);
                    if (excludedQuantity > 0) {
                        logger.Info("{0} quantity remaining", excludedQuantity);
                        var newArticle = originalArticle.Clone();
                        newArticle.Quantity = excludedQuantity;
                        actionResult.NewArticleData = newArticle;
                    }
                }

                var toMark = markedArticlesByIndex.TryGetValueWithDefault(index);
                HandleToMark(promotion, toMark, actionResult);
                result.ArticleResultPerIndex[index] = actionResult;
            }

            foreach (var action in promotion.Actions) {
                if (action.Type != PromotionActionType.Voucher) { continue; }
                if (includedByIndex.IsEmpty()) {
                    logger.Warn("Skipping {0} action because there were no eligible articles", nameof(PromotionActionType.Voucher));
                    continue;
                }
                var voucherArticleID = settings.VoucherSettings.VoucherArticleID;
                var voucherValue = action.Amount.To<decimal>().RoundTo(2);
                var voucherDiscount = action.GetValue<decimal>(1).RoundTo(2);
                if (voucherDiscount != 0) {
                    var valueToDiscount = CalculateDiscountValue(promotionProcessInfo, indexedArticleData, includedByIndex, voucherDiscount);
                    result.VoucherWithDiscount.Add(new PromotionAddVoucherWithDiscountAction {
                        PromotionID = promotion.PromotionID,
                        VoucherArticleID = voucherArticleID,
                        Discount = voucherDiscount,
                        CalculatedDiscountValue = valueToDiscount,
                        ValidFrom = ParseDateFromValue(action.GetValue<string>(2), "yyyy-MM-dd"),
                        ValidUntil = ParseDateFromValue(action.GetValue<string>(3), "yyyy-MM-dd HH:mm"),
                    });
                    logger.Info("Added {0} action (art:{3}) with discount {1} (value: {2}).", nameof(PromotionActionType.Voucher), voucherDiscount, valueToDiscount, voucherArticleID);
                } else if (voucherValue != 0) {
                    result.VoucherWithDiscountValue.Add(new PromotionAddVoucherWithDiscountValueAction {
                        PromotionID = promotion.PromotionID,
                        VoucherArticleID = voucherArticleID,
                        DiscountValue = voucherValue,
                        ValidFrom = ParseDateFromValue(action.GetValue<string>(2), "yyyy-MM-dd"),
                        ValidUntil = ParseDateFromValue(action.GetValue<string>(3), "yyyy-MM-dd HH:mm"),
                    });
                    logger.Info("Added {0} action (art:{2}) with discount value: {1}.", nameof(PromotionActionType.Voucher), voucherValue, voucherArticleID);
                }
            }

            ProcessAddArticleActions(promotionProcessInfo, numberOfTimes, result);
            MarkArticlesInDefaultConditions(conditionResult, numberOfTimes, indexedArticleData, result, promotionProcessInfo, logger);

            return result;
        }

        private static (List<PromotionConditionResult> conditionResultToProcess, int numberOfTimes) GetConditionResultToProcess(PromotionCombinedConditionResult conditionResult, PromotionParameters parameters) {
            var logger = GetLogger(parameters);
            List<PromotionConditionResult> conditionResultToProcess;
            var defaultNumberOfTimes = PromotionCalculationHelpers.CalculateNumberOfTimes(conditionResult.DefaultConditionResult);
            int? actionNumberOfTimes;
            if (conditionResult.ActionConditionResult == null) {
                logger.Info("No action condition result present. Default number of times: {0}.", (!defaultNumberOfTimes.HasValue ? "null" : defaultNumberOfTimes.Value.ToString()));
                conditionResultToProcess = conditionResult.DefaultConditionResult;
                actionNumberOfTimes = defaultNumberOfTimes;
            } else {
                conditionResultToProcess = conditionResult.ActionConditionResult;
                actionNumberOfTimes = PromotionCalculationHelpers.CalculateNumberOfTimes(conditionResultToProcess);
                logger.Info("Action condition result present. Default number of times: {1}, Action number of times: {0}.", actionNumberOfTimes, (!defaultNumberOfTimes.HasValue ? "null" : defaultNumberOfTimes.Value.ToString()));
            }
            var numberOfTimes = GetMinNumberOfTimes(actionNumberOfTimes, defaultNumberOfTimes);
            if (conditionResult.OverrulingNumberOfTimes.HasValue) {
                numberOfTimes = conditionResult.OverrulingNumberOfTimes.Value;
            }
            logger.Info("Actual number of times is {0}", numberOfTimes);
            return (conditionResultToProcess, numberOfTimes);
        }

        private static void MarkArticlesInDefaultConditions(PromotionCombinedConditionResult conditionResult, int numberOfTimes,
                                                            Dictionary<int, ArticleData> indexedArticleData, PromotionProcessResult result,
                                                            PromotionProcessInfo promotionProcessInfo, ILogger logger) {
            if (conditionResult.ActionConditionResult == null) {
                return;
            }
            var promotion = promotionProcessInfo.Promotion;
            var promotionParameters = promotionProcessInfo.Parameters;
            logger.Info("Process action condition result");
            var defaultEligibleParameters = new PromotionEligibleArticlesParameters(conditionResult.DefaultConditionResult, numberOfTimes, promotionParameters.EnablePromotionGroupingIndex) {
                ActionArticleType = 0,
                Quantity = int.MaxValue,
            };
            var defaultEligibleArticles = PromotionEligibleArticleChecker.GetEligibleArticles(defaultEligibleParameters);
            var defaultIncludedArticles = defaultEligibleArticles.Included.SelectMany(c => c).ToList();
            var defaultIncludedByIndex = defaultIncludedArticles.GroupBy(pda => pda.ArticleData.Index).ToDictionary(g => g.Key, g => g.ToList());
            if (defaultEligibleArticles.ToMark.HasAny()) {
                logger.Error("Verify");
                throw new Exception("Verify");
            }

            var defaultIndices = defaultIncludedByIndex.Keys.OrderBy(i => i).ToList();
            // mark articles that "activated" (default conditions) the promotion 
            foreach (var index in defaultIndices) {
                var originalArticle = indexedArticleData.TryGetValueWithDefault(index);
                var included = defaultIncludedByIndex.TryGetValueWithDefaultFunc(index, () => new List<EligiblePromotionArticle>(0));
                var includedQuantity = included.Sum(a => a.ArticleData.Quantity);
                if (includedQuantity == 0) {
                    logger.Info("{0} at index {1}: Skipping because of no included quantity.", originalArticle.ArticleID, index);
                    continue;
                }

                if (!result.ArticleResultPerIndex.TryGetValue(index, out var actionResult)) {
                    actionResult = new PromotionArticleProcessResult(originalArticle);
                    result.ArticleResultPerIndex.Add(index, actionResult);
                }
                var remainingQuantity = originalArticle.Quantity - actionResult.ArticleDataAdjustments.Sum(a => a.Quantity);
                if (remainingQuantity < includedQuantity) {
                    logger.Info("Ignore adding adjustments to {0} at index {1} to mark for a quantity of {2} because there is no remaining quantity ({3})", originalArticle.ArticleID, index, includedQuantity, remainingQuantity);
                    continue;
                }
                var adjustedArticle = new ArticleDataAdjustment {
                    Quantity = includedQuantity,
                    PromotionID = promotion.PromotionID
                };
                actionResult.ArticleDataAdjustments.Add(adjustedArticle);
                logger.Info("Added adjustments to {0} at index {1} to mark for a quantity of {2}", originalArticle.ArticleID, index, includedQuantity);
                var excludedQuantity = originalArticle.Quantity - includedQuantity;
                if (excludedQuantity > 0) {
                    logger.Info("{0} quantity remaining", excludedQuantity);
                    var newArticle = originalArticle.Clone();
                    newArticle.Quantity = excludedQuantity;
                    actionResult.NewArticleData = newArticle;
                }
            }
        }

        private static void HandleToMark(PromotionWrapper promotion, List<PromotionArticleData> toMark, PromotionArticleProcessResult processResult) {
            if (toMark.IsEmpty()) { return; }
            // article won't be processed by the action, but it was included in the articles that "activated" the action, so it needs to be marked as used.
            UpdateAdjustedArticleData(promotion, toMark, processResult);
        }

        private static void HandleToMark(PromotionWrapper promotion, List<EligiblePromotionArticle> toMark, PromotionArticleProcessResult processResult) {
            if (toMark.IsEmpty()) { return; }
            // article won't be processed by the action, but it was included in the articles that "activated" the action, so it needs to be marked as used.
            UpdateAdjustedArticleData(promotion, toMark, processResult);
        }

        private static void UpdateAdjustedArticleData(PromotionWrapper promotion, List<EligiblePromotionArticle> toMark, PromotionArticleProcessResult processResult) {
            foreach (var grouping in toMark.GroupBy(tm => tm.PromotionGroupingIndex)) {
                var markedAdjustedArticle = new ArticleDataAdjustment {
                    Quantity = grouping.Sum(a => a.ArticleData.Quantity),
                    PromotionID = promotion.PromotionID,
                    PromotionGroupingIndex = grouping.Key,
                };
                processResult.ArticleDataAdjustments.Add(markedAdjustedArticle);
                var remainingQuantity = processResult.OriginalArticleData.Quantity - processResult.ArticleDataAdjustments.Sum(a => a.Quantity);
                if (processResult.NewArticleData != null) {
                    if (remainingQuantity <= 0) {
                        processResult.NewArticleData = null;
                    } else {
                        processResult.NewArticleData.Quantity = remainingQuantity;
                    }
                } else {
                    if (remainingQuantity <= 0) { return; }
                    var newArticle = processResult.OriginalArticleData.Clone();
                    newArticle.Quantity = remainingQuantity;
                    processResult.NewArticleData = newArticle;
                }
            }
        }

        private static void UpdateAdjustedArticleData(PromotionWrapper promotion, List<PromotionArticleData> toMark, PromotionArticleProcessResult processResult) {
            var markedAdjustedArticle = new ArticleDataAdjustment {
                Quantity = toMark.Sum(a => a.Quantity),
                PromotionID = promotion.PromotionID,
                PromotionGroupingIndex = 0,
            };
            processResult.ArticleDataAdjustments.Add(markedAdjustedArticle);
            var remainingQuantity = processResult.OriginalArticleData.Quantity - processResult.ArticleDataAdjustments.Sum(a => a.Quantity);
            if (processResult.NewArticleData != null) {
                if (remainingQuantity <= 0) {
                    processResult.NewArticleData = null;
                } else {
                    processResult.NewArticleData.Quantity = remainingQuantity;
                }
            } else {
                if (remainingQuantity <= 0) { return; }
                var newArticle = processResult.OriginalArticleData.Clone();
                newArticle.Quantity = remainingQuantity;
                processResult.NewArticleData = newArticle;
            }
        }

        private static int GetMinNumberOfTimes(int? actionNumberOfTimes, int? defaultNumberOfTimes) {
            int numberOfTimes;
            if (!actionNumberOfTimes.HasValue) {
                numberOfTimes = defaultNumberOfTimes.GetValueOrDefault(1);
            } else if (!defaultNumberOfTimes.HasValue) {
                numberOfTimes = actionNumberOfTimes.Value;
            } else {
                numberOfTimes = Math.Min(defaultNumberOfTimes.Value, actionNumberOfTimes.Value);
            }
            return numberOfTimes;
        }

        /// <summary>
        /// Handles PromotionActions with <see cref="PromotionActionType"/> <see cref="PromotionActionType.ArticleTypePrice"/> or <see cref="PromotionActionType.ProgressiveDiscount"/>.
        /// </summary>
        /// <param name="promotionProcessInfo"></param>
        /// <param name="conditionResultToProcess"></param>
        /// <param name="numberOfTimes"></param>
        /// <param name="indexedArticleData"></param>
        /// <returns></returns>
        private static PromotionProcessResult HandleActionsWithSortingPriority(PromotionProcessInfo promotionProcessInfo, List<PromotionConditionResult> conditionResultToProcess,
                                                                               int numberOfTimes, Dictionary<int, ArticleData> indexedArticleData) {
            var promotion = promotionProcessInfo.Promotion;
            var promotionParameters = promotionProcessInfo.Parameters;
            var logger = GetLogger(promotionParameters);

            var groupedActions = promotion.Actions.GroupBy(a => a.Type).ToDictionary(a => a.Key, a => a.First());
            var articleTypePriceAction = groupedActions.TryGetValueWithDefault(PromotionActionType.ArticleTypePrice);
            var progressiveDiscountAction = groupedActions.TryGetValueWithDefault(PromotionActionType.ProgressiveDiscount);
            if (articleTypePriceAction == null && progressiveDiscountAction == null) {
                logger.Warn("No action with type {0} or {1} found.", nameof(PromotionActionType.ArticleTypePrice), nameof(PromotionActionType.ProgressiveDiscount));
                return null;
            }
            var includedArticles = ConditionChecker.GetValidArticles(conditionResultToProcess);
            var includedByIndex = includedArticles.GroupBy(pda => pda.Index).ToDictionary(g => g.Key, g => g.ToList());

            var indices = includedByIndex.Keys.OrderBy(i => i).ToList();

            var result = new PromotionProcessResult();

            if (articleTypePriceAction != null) {
                logger.Info("Processing article type price action");
                var actionArticleType = articleTypePriceAction.Value2.To<PromotionActionArticleType>();
                var primaryActionArticleType = articleTypePriceAction.Value5.To<PromotionActionArticleType?>();
                var actionQuantity = articleTypePriceAction.GetValue<decimal>(1).To<int>();
                // fill the articlesToAction list by creating an eligible list of items, gathered from eligible groups from each condition
                var promotionEligibleArticlesParameters = new PromotionEligibleArticlesParameters(conditionResultToProcess, numberOfTimes, promotionParameters.EnablePromotionGroupingIndex) {
                    ActionArticleType = actionArticleType,
                    PrimaryActionArticleType = primaryActionArticleType ?? actionArticleType,
                    Quantity = actionQuantity,
                };
                var eligibleArticles = PromotionEligibleArticleChecker.GetEligibleArticles(promotionEligibleArticlesParameters);
                var articlesToAction = new List<EligiblePromotionArticle>();
                foreach (var eligibleArticleList in eligibleArticles.Included) {
                    articlesToAction.AddRange(eligibleArticleList);
                }
                var allFrom = eligibleArticles.AllFrom;
                logger.Info("Found {0} to action, {1} to mark", articlesToAction.Count, eligibleArticles.ToMark.Count);
                var discountedArticlesGroupedByIndex = articlesToAction.GroupBy(da => da.ArticleData.Index).ToDictionary(da => da.Key, da => da.ToList());
                var markedArticlesByIndex = eligibleArticles.ToMark.GroupBy(pda => pda.ArticleData.Index).ToDictionary(g => g.Key, g => g.ToList());
                var actionPrice = articleTypePriceAction.Amount.To<decimal?>();
                var actionDiscount = articleTypePriceAction.GetValue<decimal?>(3);
                var actionDiscountAmount = articleTypePriceAction.GetValue<decimal?>(4);
                // loop all indices to process the action
                foreach (var index in indices) {
                    var originalArticle = indexedArticleData.TryGetValueWithDefault(index);
                    if (originalArticle.VoucherType != PromotionVoucherType.None) {
                        logger.Info("{0} at index {1}: Skipping action because voucher type is {2}.", originalArticle.ArticleID, index, originalArticle.VoucherType);
                        continue;
                    }
                    var actionResult = new PromotionArticleProcessResult(originalArticle);
                    var included = discountedArticlesGroupedByIndex.TryGetValueWithDefault(index);
                    if (included != null) {
                        var includedQuantity = 0m;

                        foreach (var includedData in included) {
                            var promotionArticleData = includedData.ArticleData;
                            var adjustedArticle = new ArticleDataAdjustment {
                                Quantity = promotionArticleData.Quantity,
                                PromotionID = promotion.PromotionID,
                                PromotionGroupingIndex = includedData.PromotionGroupingIndex,
                            };
                            if (actionDiscount.GetValueOrDefault() != 0) {
                                adjustedArticle.SetDiscount(1, actionDiscount);
                            } else if (actionDiscountAmount.GetValueOrDefault() != 0) {
                                adjustedArticle.DiscountAmount = actionDiscountAmount;
                            } else if (actionPrice.HasValue) {
                                adjustedArticle.BasePrice = actionPrice;
                            }
                            actionResult.ArticleDataAdjustments.Add(adjustedArticle);
                            includedQuantity += adjustedArticle.Quantity;
                        }
                        logger.Info("Added adjustments to {0} at index {1} for a quantity of {2}", originalArticle.ArticleID, index, includedQuantity);
                        var excludedQuantity = originalArticle.Quantity - includedQuantity;
                        if (excludedQuantity > 0) {
                            logger.Info("{0} quantity remaining", excludedQuantity);
                            var newArticle = originalArticle.Clone();
                            newArticle.Quantity = excludedQuantity;
                            actionResult.NewArticleData = newArticle;
                        }
                    }
                    if (!allFrom) {
                        var toMark = markedArticlesByIndex.TryGetValueWithDefault(index);
                        HandleToMark(promotion, toMark, actionResult);
                    }

                    result.ArticleResultPerIndex[index] = actionResult;
                }
                return result;
            } else {
                logger.Info("Processing progressive discount action");
                var actionArticleType = progressiveDiscountAction.Value1.To<PromotionActionArticleType>();
                var isConsolidated = actionArticleType == PromotionActionArticleType.MostExpensiveLineConsolidated;
                // fill the discountedArticles list containing a promotion article data and the discount to give
                var discounts = Enumerable.Range(2, 3).Select(i => progressiveDiscountAction.GetValue<decimal>(i).RoundTo(2)).ToList();
                while (discounts[discounts.Count - 1] == 0) {
                    discounts.RemoveAt(discounts.Count - 1);
                }
                if (discounts.Count > 0 && discounts[0] == 0) {
                    logger.Warn("Invalid discount setup, first discount is 0.");
                    return result;
                }
                var discountedArticles = new KeyValueList<PromotionArticleData, decimal>();
                //var articlesToMark = new HashSet<PromotionArticleData>();
                foreach (var conditionResult in conditionResultToProcess) {
                    var sorted = PromotionArticleTypeSorter.Sort(conditionResult.Included.ToList(), actionArticleType, int.MaxValue);
                    if (conditionResult.Criterion.Grouping == PromotionGrouping.Per) {
                        return result; // per is not supported when using ProgressiveDiscount actions
                    }
                    if (isConsolidated || actionArticleType == PromotionActionArticleType.MostExpensiveLine) {
                        var groupedList = sorted.GroupBy(a => isConsolidated ? a.ArticleID : a.Index).ToList();
                        var minCount = Math.Min(groupedList.Count, discounts.Count);
                        for (var index = 0; index < minCount; index++) {
                            foreach (var art in groupedList[index]) {
                                discountedArticles.Add(art, discounts[index]);
                            }
                        }
                    } else {
                        var minCount = Math.Min(sorted.Count, discounts.Count);
                        for (var index = 0; index < minCount; index++) {
                            discountedArticles.Add(sorted[index], discounts[index]);
                        }
                    }
                }
                var discountedArticlesGroupedByIndex = discountedArticles.GroupBy(da => da.Key.Index).ToDictionary(da => da.Key, da => da.ToList());
                //var markedArticlesByIndex = articlesToMark.GroupBy(pda => pda.Index).ToDictionary(g => g.Key, g => g.ToList());
                // loop all indices to process the action
                foreach (var index in indices) {
                    var originalArticle = indexedArticleData.TryGetValueWithDefault(index);
                    if (originalArticle.VoucherType != PromotionVoucherType.None) {
                        logger.Info("{0} at index {1}: Skipping action because voucher type is {2}.", originalArticle.ArticleID, index, originalArticle.VoucherType);
                        continue;
                    }
                    var actionResult = new PromotionArticleProcessResult(originalArticle);
                    var included = discountedArticlesGroupedByIndex.TryGetValueWithDefault(index);
                    if (included != null) {
                        var includedQuantity = 0m;
                        foreach (var includedData in included) {
                            var promotionArticleData = includedData.Key;
                            var adjustedArticle = new ArticleDataAdjustment {
                                Quantity = promotionArticleData.Quantity,
                                PromotionID = promotion.PromotionID
                            };
                            adjustedArticle.SetDiscount(1, includedData.Value);
                            actionResult.ArticleDataAdjustments.Add(adjustedArticle);
                            includedQuantity += adjustedArticle.Quantity;
                        }
                        logger.Info("Added adjustments to {0} at index {1} for a quantity of {2}", originalArticle.ArticleID, index, includedQuantity);
                        var excludedQuantity = originalArticle.Quantity - includedQuantity;
                        if (excludedQuantity > 0) {
                            logger.Info("{0} quantity remaining", excludedQuantity);
                            var newArticle = originalArticle.Clone();
                            newArticle.Quantity = excludedQuantity;
                            actionResult.NewArticleData = newArticle;
                        }
                    }
                    //var toMark = markedArticlesByIndex.TryGetValueWithDefault(index);
                    //HandleToMark(promotion, toMark, actionResult);

                    result.ArticleResultPerIndex[index] = actionResult;
                }
                return result;
            }
        }

        private static void ProcessAddArticleActions(PromotionProcessInfo promotionProcessInfo, int numberOfTimes, PromotionProcessResult result) {
            var promotion = promotionProcessInfo.Promotion;
            var logger = GetLogger(promotionProcessInfo.Parameters);
            foreach (var action in promotion.Actions) {
                switch (action.Type) {
                    case PromotionActionType.AddArticleWithPrice:
                        var priceAction = new PromotionAddArticleWithPriceAction {
                            PromotionID = promotion.PromotionID,
                            ArticleID = action.GetValue<int>(1),
                            Price = action.GetValue<decimal>(3).RoundTo(2),
                            Quantity = action.Amount.To<decimal>().To<int>() * numberOfTimes,
                        };
                        logger.Info("Added {0} action ({3}x{1} at {2})", nameof(PromotionActionType.AddArticleWithPrice), priceAction.ArticleID, priceAction.Price, priceAction.Quantity);
                        result.ArticlesWithPrice.Add(priceAction);
                        break;
                    case PromotionActionType.AddArticleWithDiscount:
                        var discountAction = new PromotionAddArticleWithDiscountAction {
                            PromotionID = promotion.PromotionID,
                            ArticleID = action.GetValue<int>(1),
                            Discount = action.GetValue<decimal>(3).RoundTo(2),
                            Quantity = action.Amount.To<decimal>().To<int>() * numberOfTimes,
                        };
                        logger.Info("Added {0} action ({3}x{1} at {2}%)", nameof(PromotionActionType.AddArticleWithDiscount), discountAction.ArticleID, discountAction.Discount, discountAction.Quantity);
                        result.ArticlesWithDiscount.Add(discountAction);
                        break;
                }
            }
        }

        private static bool IsEligibleForDiscountValueCalculation(PromotionProcessInfo promotionProcessInfo, ArticleData article, int index) {
            var settings = promotionProcessInfo.Settings;
            var logger = GetLogger(promotionProcessInfo.Parameters);
            if (article == null) {
                logger.Warn("No article data found for index {0}", index);
                return false;
            }
            if (article.VoucherType == PromotionVoucherType.None) {
                if (article.IsPriceChanged && !(settings.IncludePriceChangedGenericArticle && article.IsGenericArticle)) {
                    logger.Warn("{0} at index {1} is not eligible for processing because {2} is {3}.", article.ArticleID, article.Index, nameof(article.IsPriceChanged), article.IsPriceChanged);
                    return false;
                }
                // todo: which price ? discounts?
                if (settings.SkipWithDiscount && article.HasDiscount) {
                    logger.Warn("{0} at index {1} is not eligible for processing because {2} is enabled and {3} is {4}.",
                                article.ArticleID, article.Index, nameof(settings.SkipWithDiscount), nameof(article.HasDiscount), article.HasDiscount);
                    return false;
                }
                if (settings.SkipWithPromotionPrice && article.HasPromotionPrice) {
                    logger.Warn("{0} at index {1} is not eligible for processing because {2} is enabled and {3} is {4}.",
                                article.ArticleID, article.Index, nameof(settings.SkipWithPromotionPrice), nameof(article.HasPromotionPrice), article.HasPromotionPrice);
                    return false;
                }
                if (settings.SkipNoDiscountArticle && article.IsNoDiscount) {
                    logger.Warn("{0} at index {1} is not eligible for processing because {2} is enabled and {3} is {4}.",
                                article.ArticleID, article.Index, nameof(settings.SkipNoDiscountArticle), nameof(article.IsNoDiscount), article.IsNoDiscount);
                    return false;
                }
                if (settings.SkipNoPoints && article.IsNoPoints) {
                    logger.Warn("{0} at index {1} is not eligible for processing because {2} is enabled and {3} is {4}.",
                                article.ArticleID, article.Index, nameof(settings.SkipNoPoints), nameof(article.IsNoPoints), article.IsNoPoints);
                    return false;
                }
            }
            if (settings.SkipGiftVoucher && article.VoucherType == PromotionVoucherType.Gift) {
                logger.Warn("{0} at index {1} is not eligible for processing because {2} is enabled and {3} is {4}.",
                            article.ArticleID, article.Index, nameof(settings.SkipGiftVoucher), nameof(article.VoucherType), nameof(PromotionVoucherType.Gift));
                return false;
            }
            if (settings.SkipDiscountVoucher && article.VoucherType == PromotionVoucherType.Discount) {
                logger.Warn("{0} at index {1} is not eligible for processing because {2} is enabled and {3} is {4}.",
                            article.ArticleID, article.Index, nameof(settings.SkipDiscountVoucher), nameof(article.VoucherType), nameof(PromotionVoucherType.Discount));
                return false;
            }
            if (article.VoucherType == PromotionVoucherType.Value || article.VoucherType == PromotionVoucherType.Other) {
                logger.Warn("{0} at index {1} is not eligible for processing because {2} is {3}.",
                            article.ArticleID, article.Index, nameof(article.VoucherType), PromotionVoucherType.Value);
                return false;
            }
            logger.Info("{0} at index {1} is eligible for discount value calculation.", article.ArticleID, article.Index);
            return true;
        }

        private static bool IsEligibleForDiscountValueCalculation(PromotionProcessInfo promotionProcessInfo, Dictionary<int, ArticleData> articles, int index) {
            var article = articles.TryGetValueWithDefault(index);
            return IsEligibleForDiscountValueCalculation(promotionProcessInfo, article, index);
        }

        private static decimal CalculateDiscountValue(PromotionProcessInfo promotionProcessInfo, Dictionary<int, ArticleData> articles, Dictionary<int, List<PromotionArticleData>> includedByIndex, decimal voucherDiscount) {
            var logger = GetLogger(promotionProcessInfo.Parameters);
            logger.Info("Start calculating discount value");
            var totalValue = 0m;
            foreach (var included in includedByIndex) {
                var article = articles.TryGetValueWithDefault(included.Key);
                var isEligibleForCalculation = IsEligibleForDiscountValueCalculation(promotionProcessInfo, article, included.Key);
                if (!isEligibleForCalculation) {
                    continue;
                }
                var quantityIncluded = included.Value.Sum(a => a.Quantity);
                var includedValue = (quantityIncluded * article.Price);
                logger.Info("{0} at index {1} is counted for a total of {2} ({3}x {4}).", article.ArticleID, article.Index, includedValue, quantityIncluded, article.Price);
                totalValue += includedValue;
            }
            var voucherValue = (totalValue * (voucherDiscount / 100m)).RoundTo(2);
            logger.Info("Result: {0}, Total: {1}, Discount: {2}", voucherValue, totalValue, voucherDiscount);
            return voucherValue;
        }

        private static decimal CalculateDiscountValue(PromotionProcessInfo promotionProcessInfo, Dictionary<int, ArticleData> articles, Dictionary<int, List<EligiblePromotionArticle>> includedByIndex, decimal voucherDiscount) {
            var logger = GetLogger(promotionProcessInfo.Parameters);
            logger.Info("Start calculating discount value");
            var totalValue = 0m;
            foreach (var included in includedByIndex) {
                var article = articles.TryGetValueWithDefault(included.Key);
                var isEligibleForCalculation = IsEligibleForDiscountValueCalculation(promotionProcessInfo, article, included.Key);
                if (!isEligibleForCalculation) {
                    continue;
                }
                var quantityIncluded = included.Value.Sum(a => a.ArticleData.Quantity);
                var includedValue = (quantityIncluded * article.Price);
                logger.Info("{0} at index {1} is counted for a total of {2} ({3}x {4}).", article.ArticleID, article.Index, includedValue, quantityIncluded, article.Price);
                totalValue += includedValue;
            }
            var voucherValue = (totalValue * (voucherDiscount / 100m)).RoundTo(2);
            logger.Info("Result: {0}, Total: {1}, Discount: {2}", voucherValue, totalValue, voucherDiscount);
            return voucherValue;
        }

        private PromotionConditionParameters GetConditionParameters(PromotionWrapper promotion,
                                                                    List<PromotionArticleData> items,
                                                                    PromotionConditionKind kind) {
            var conditionParameters = new PromotionConditionParameters(MinimumGroupLevel, MaximumGroupLevel) {
                Promotion = promotion,
                Kind = kind,
                Data = items,
            };
            return conditionParameters;
        }

        /// <summary>
        /// Converts a list of <see cref="ArticleData"/> to a list of single quantity <see cref="PromotionArticleData"/> objects. Each <see cref="PromotionArticleData"/> gets a unique ID.
        /// </summary>
        /// <param name="articleData"></param>
        /// <param name="promotionArticleContainer"></param>
        /// <returns>Returns a list of <see cref="PromotionArticleData"/> objects, created from a list of <see cref="ArticleData"/> objects.</returns>
        private static List<PromotionArticleData> GetPromotionArticleData(List<ArticleData> articleData, PromotionArticleContainer promotionArticleContainer) {
            var items = new List<PromotionArticleData>();
            var id = 0;
            foreach (var articleGrouping in articleData.GroupBy(a => a.ArticleID)) {
                var promotionArticle = promotionArticleContainer.Get(articleGrouping.Key);
                if (promotionArticle == null) { continue; }
                foreach (var art in articleGrouping) {
                    var itemCount = Math.Ceiling(Math.Abs(art.Quantity)).To<int>();
                    var remainingCount = art.Quantity;
                    var isNegative = art.Quantity < 0;
                    for (var i = 0; i < itemCount; i++) {
                        var isNotLast = i < itemCount - 1;
                        var itemQuantity = isNotLast ? (isNegative ? -1 : 1) : remainingCount;
                        var promotionData = new PromotionArticleData(id, art.ArticleID, art.Index) {
                            Quantity = itemQuantity,
                            PurchasePrice = art.PurchasePrice,
                            Data = promotionArticle,
                            Weight = art.Weight?.NullIf(0) ?? promotionArticle.Weight,
                            IsWeighted = art.IsWeighted,
                        };
                        if (art.HasPromotionPrice || art.PromotionID == 0 || art.IsPriceChanged || art.SkipPromotionCalculation) {
                            promotionData.Price = art.Price;
                        } else {
                            promotionData.Price = art.BasePrice;
                        }
                        items.Add(promotionData);
                        if (isNotLast) {
                            remainingCount -= (isNegative ? -1 : 1);
                        }
                        id += 1;
                    }
                }
            }
            return items;
        }

        private static bool CanProcessActionVoucher(PromotionProcessInfo promotionProcessInfo) {
            var settings = promotionProcessInfo.Settings;
            var parameters = promotionProcessInfo.Parameters;
            var logger = GetLogger(parameters);
            if (parameters.ExcludeVoucherPromotions) {
                logger.Debug("Cannot process promotion with action voucher because {0} is {1}.", nameof(parameters.ExcludeVoucherPromotions), parameters.ExcludeVoucherPromotions);
                return false;
            }
            var excludeDefaultCustomerFromVoucherAction = settings.VoucherSettings.ExcludeDefaultCustomerFromVoucherAction;
            if (excludeDefaultCustomerFromVoucherAction && parameters.ContactID == settings.DefaultCustomerID) {
                logger.Debug("Cannot process promotion with action voucher because {0} is true and {1} is the default customer.",
                             nameof(settings.VoucherSettings.ExcludeDefaultCustomerFromVoucherAction), nameof(parameters.ContactID));
                return false;
            }
            var excludedDocumentTypeIDsFromVoucherAction = settings.VoucherSettings.ExcludedDocumentTypeIDsFromVoucherAction;
            if (excludedDocumentTypeIDsFromVoucherAction.Contains(parameters.DocumentTypeID)) {
                logger.Debug("Cannot process promotion with action voucher because {0} ({1}) contains {2}",
                             nameof(settings.VoucherSettings.ExcludedDocumentTypeIDsFromVoucherAction), string.Join(",", settings.VoucherSettings.ExcludedDocumentTypeIDsFromVoucherAction), parameters.DocumentTypeID);
                return false;
            }
            var voucherArticleID = settings.VoucherSettings.VoucherArticleID;
            if (voucherArticleID == 0) {
                logger.Debug("Cannot process promotion with action voucher because {0} is 0.", nameof(settings.VoucherSettings.VoucherArticleID));
            }
            return voucherArticleID != 0;
        }

        private static bool CanProcessParameters(BasePromotionParameters parameters, PromotionSettings settings) {
            var logger = GetLogger(parameters);
            if (settings.ExcludedDocumentTypeIDs.Contains(parameters.DocumentTypeID)) {
                logger.Warn("Can't process parameters because {0} is present in {1} with values {2}.", parameters.DocumentTypeID, nameof(settings.ExcludedDocumentTypeIDs), string.Join(",", settings.ExcludedDocumentTypeIDs));
                return false;
            }
            if (settings.ExcludedContactIDs.Contains(parameters.ContactID)) {
                logger.Warn("Can't process parameters because {0} is present in {1} with values {2}.", parameters.ContactID, nameof(settings.ExcludedContactIDs), string.Join(",", settings.ExcludedContactIDs));
                return false;
            }
            logger.Info("Can process parameters");
            return true;
        }

        private bool CanChooseBestPromotion(PromotionProcessInfo promotionProcessInfo) {
            var promotion = promotionProcessInfo.Promotion;
            var parameters = promotionProcessInfo.Parameters;
            var logger = GetLogger(parameters);
            if (promotion.AlwaysExecute) {
                logger.Warn("Promotion not eligible for {0} calculation because {1} is {2}.", nameof(PromotionParameters.ChooseBestPromotion), nameof(promotion.AlwaysExecute), promotion.AlwaysExecute);
                return false;
            }
            if (parameters.ChooseBestPromotion && !promotion.ExcludeFromBestPromotion) {
                logger.Info("Promotion eligible for {0} calculation", nameof(PromotionParameters.ChooseBestPromotion));
                return true;
            }
            if (!parameters.ChooseBestPromotion) {
                logger.Warn("Promotion not eligible for {0} calculation because {0} is {1}.", nameof(PromotionParameters.ChooseBestPromotion), parameters.ChooseBestPromotion);
            }
            if (promotion.ExcludeFromBestPromotion) {
                logger.Warn("Promotion not eligible for {0} calculation because {1} is {2}.", nameof(PromotionParameters.ChooseBestPromotion), nameof(promotion.ExcludeFromBestPromotion), promotion.ExcludeFromBestPromotion);
            }
            return false;
        }

        private static (List<ArticleData> FilteredArticles, List<ArticleData> AlwaysExecuteArticles) Filter(PromotionParameters parameters, PromotionSettings settings) {
            var logger = GetLogger(parameters);
            logger.Info("Filtering data. Started with {0} articles.", parameters.Data.Count);
            var filteredArticles = new List<ArticleData>();
            var alwaysExecuteArticles = new List<ArticleData>();
            foreach (var article in parameters.Data) {
                if (article.SkipPromotionCalculation) {
                    logger.Warn("{0} at index {1} is not eligible for processing because {2} is {3}.", article.ArticleID, article.Index, nameof(article.SkipPromotionCalculation), article.SkipPromotionCalculation);
                    continue;
                }
                if (article.VoucherType == PromotionVoucherType.Other || article.VoucherType == PromotionVoucherType.Value) {
                    logger.Warn("{0} at index {1} is not eligible for processing because {2} is {3}.", article.ArticleID, article.Index, nameof(article.VoucherType), article.VoucherType);
                    continue;
                }
                if (!settings.IncludeWeightedArticles && article.IsWeighted) {
                    logger.Warn("{0} at index {1} is not eligible for processing because {2} is {3} and {4} is {5}.", article.ArticleID, article.Index, nameof(article.IsWeighted), article.IsWeighted, nameof(settings.IncludeWeightedArticles), settings.IncludeWeightedArticles);
                    continue;
                }
                if (article.IsAddedByPromotion) {
                    logger.Warn("{0} at index {1} is not eligible for processing because {2} is {3}.", article.ArticleID, article.Index, nameof(article.IsAddedByPromotion), article.IsAddedByPromotion);
                    continue;
                }
                if (settings.SkipNoDiscountArticle && article.IsNoDiscount) {
                    logger.Warn("{0} at index {1} is not eligible for processing because {2} is enabled and {3} is {4}.",
                                article.ArticleID, article.Index, nameof(settings.SkipNoDiscountArticle), nameof(article.IsNoDiscount), article.IsNoDiscount);
                    continue;
                }
                try {
                    if (!parameters.ChooseBestPromotion) {
                        if (settings.SkipWithDiscount && article.HasDiscount) {
                            logger.Warn("{0} at index {1} is not eligible for processing because {2} is enabled and {3} is {4}.",
                                        article.ArticleID, article.Index, nameof(settings.SkipWithDiscount), nameof(article.HasDiscount), article.HasDiscount);
                            continue;
                        }
                        if (settings.SkipWithPromotionPrice && article.HasPromotionPrice) {
                            logger.Warn("{0} at index {1} is not eligible for processing because {2} is enabled and {3} is {4}.",
                                        article.ArticleID, article.Index, nameof(settings.SkipWithPromotionPrice), nameof(article.HasPromotionPrice), article.HasPromotionPrice);
                            continue;
                        }
                    }
                } finally {
                    alwaysExecuteArticles.Add(article);
                }
                if (article.IsPriceChanged && !article.IsFixedPrice && !(settings.IncludePriceChangedGenericArticle && article.IsGenericArticle)) {
                    logger.Warn("{0} at index {1} is not eligible for processing because {2} is {3}.", article.ArticleID, article.Index, nameof(article.IsPriceChanged), article.IsPriceChanged);
                    continue;
                }
                filteredArticles.Add(article);
            }
            logger.Info("Filtered data. Ended with {0} eligible articles.", parameters.Data.Count);
            return (filteredArticles, alwaysExecuteArticles);
        }

        private static (List<ArticleData> FilteredArticles, List<ArticleData> AlwaysExecuteArticles) Filter(List<PromotionParameters> parameterList, PromotionSettings settings) {
            var filteredArticles = new List<ArticleData>();
            var alwaysExecuteArticles = new List<ArticleData>();
            foreach (var parameters in parameterList) {
                var temp = Filter(parameters, settings);
                filteredArticles.AddRange(temp.FilteredArticles);
                alwaysExecuteArticles.AddRange(temp.AlwaysExecuteArticles);
            }
            return (filteredArticles, alwaysExecuteArticles);
        }

        private static void UpdateActions<T>(Dictionary<Tuple<int, int>, List<ArticleData>> dataAddedByPromotion,
                                             List<T> items, PromotionProcessResult result, Func<ArticleData, T, bool> allCheck) where T : PromotionAddArticle {
            foreach (var priceAction in items) {
                var articleData = dataAddedByPromotion.TryGetValueWithDefault(Tuple.Create(priceAction.PromotionID, priceAction.ArticleID));
                if (articleData.IsEmpty()) { continue; }
                var quantitySeen = articleData.Sum(article => article.Quantity);
                var haveSameDiscount = articleData.All(article => allCheck(article, priceAction));
                if (priceAction.Quantity < quantitySeen || !haveSameDiscount) {
                    result.ArticlesToRemove.AddRange(articleData);
                    break;
                }
                priceAction.QuantityAlreadyPresent = quantitySeen;
                priceAction.Quantity -= quantitySeen;
            }
        }

        private static DateTime? ParseDateFromValue(string value, string format) {
            if (value.IsNullOrWhiteSpace()) { return null; }
            if (!DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var temp)) {
                return null;
            }
            if (temp == default) {
                return null;
            }
            return temp;
        }

        private static ActivatedPromotionResult GetActivatedPromotionResult(PromotionWrapper promotion, int numberOfTimes, PromotionSettings settings) {
            DateTime validFrom;
            DateTime validTo;
            if (promotion.DaysValidAfterCreation != 0) {
                validFrom = DateTime.Today;
                validTo = validFrom.AddDays(promotion.DaysValidAfterCreation);
            } else {
                if (promotion.ActionVoucherFrom.HasValue && promotion.ActionVoucherTo.HasValue) {
                    validFrom = promotion.ActionVoucherFrom.Value;
                    validTo = promotion.ActionVoucherTo.Value;
                } else {
                    validFrom = promotion.To;
                    validTo = validFrom.AddMonths(settings.ActionVoucherSettings?.NumberOfMonthsValidAfterPromotionEnds ?? 0);
                }
            }
            return new ActivatedPromotionResult(promotion.PromotionID, numberOfTimes, promotion.Name, validFrom.Date, validTo.ToEndOfDay());
        }
    }
}