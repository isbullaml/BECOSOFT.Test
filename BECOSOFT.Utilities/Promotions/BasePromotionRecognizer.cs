using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Models.Promotions;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Promotions {
    public abstract class BasePromotionRecognizer {
        public PromotionRecognitionResult Recognize(PromotionRecognitionParameters parameters) {
            var promotions = GetValidPromotions(parameters);
            if (parameters.ConditionsToExclude.HasAny()) {
                promotions = promotions.Where(p => !p.Conditions.Any(pc => parameters.ConditionsToExclude.Contains(pc.ConditionType))).ToList();
            }
            if (parameters.ActionsToExclude.HasAny()) {
                promotions = promotions.Where(p => !p.Actions.Any(pa => parameters.ActionsToExclude.Contains(pa.Type))).ToList();
            }

            var result = new PromotionRecognitionResult();

            var hasTagsCondition = promotions.Any(p => p.PromotionConditions.Any(pc => pc.IsTagCondition()));
            var promotionArticleContainer = UpdatePromotionArticleData(parameters.PromotionArticleContainer, parameters.ArticleIDs, hasTagsCondition);
            parameters.PromotionArticleContainer = promotionArticleContainer;

            var recognitions = new HashSet<RecognizedArticle>();
            foreach (var promotion in promotions) {
                var defaultConditions = promotion.GetConditions(PromotionConditionKind.Default);
                if (defaultConditions.IsEmpty()) { continue; }

                if (!parameters.IncludeMultiConditionPromotions && GetConditionCount(defaultConditions) > 1) {
                    continue;
                }

                var actionConditions = promotion.GetConditions(PromotionConditionKind.Action);
                if (!parameters.IncludeMultiConditionPromotions && GetConditionCount(actionConditions) > 1) {
                    continue;
                }

                FillRecognizedArticles(promotion, defaultConditions, promotionArticleContainer, recognitions);
            }
            result.Recognitions.AddRange(recognitions);
            foreach (var recognizedArticle in recognitions) {
                recognizedArticle.RecognizedPromotions = recognizedArticle.RecognizedPromotions.ToDistinctList();
            }
            return result;
        }

        private static void FillRecognizedArticles(PromotionWrapper promotion, List<PromotionConditionWrapper> conditions,
                                                   PromotionArticleContainer promotionArticleContainer, HashSet<RecognizedArticle> recognitions) {
            var groupedDefaultConditions = GetGroupedConditions(conditions);

            foreach (var condition in groupedDefaultConditions) {
                foreach (var criterion in condition) {
                    if (criterion.TypeOperator == PromotionTypeOperator.DifferentFrom) { continue; }
                    foreach (var articleID in promotionArticleContainer.ArticleIDs) {
                        var article = promotionArticleContainer.Get(articleID);
                        var testResult = CriterionChecker.CheckCriterion(criterion, article);
                        if (!testResult) { continue; }

                        var test = new RecognizedArticle(articleID);
                        if (!recognitions.TryGetValue(test, out var existing)) {
                            recognitions.Add(test);
                            existing = test;
                        }
                        var recognizedAction = new PromotionRecognition(promotion.PromotionID, promotion.Name);
                        existing.RecognizedPromotions.Add(recognizedAction);
                    }
                }
            }
        }

        public PromotionArticleContainer GetPromotionArticleData(List<int> articleIDs, bool hasTagsCondition) {
            return UpdatePromotionArticleData(null, articleIDs, hasTagsCondition);
        }

        /// <summary>
        /// Retrieve the <see cref="PromotionConditionWrapper"/> objects, grouped per <see cref="PromotionConditionWrapper.Group"/>.
        /// </summary>
        /// <param name="promotionConditions">Conditions to group</param>
        /// <returns></returns>
        private static List<List<PromotionConditionWrapper>> GetGroupedConditions(List<PromotionConditionWrapper> promotionConditions) {
            var temp = new Dictionary<string, List<PromotionConditionWrapper>>();
            foreach (var condition in promotionConditions) {
                if (!temp.TryGetValue(condition.Group, out _)) {
                    temp.Add(condition.Group, new List<PromotionConditionWrapper>());
                }

                temp[condition.Group].Add(condition);
                if (condition.ConditionType == PromotionConditionType.AllArticles) { }
            }

            var conditions = temp.OrderBy(p => p.Key).Select(p => p.Value).ToList();
            return conditions;
        }

        private static int GetConditionCount(List<PromotionConditionWrapper> conditions) {
            var groups = new HashSet<string>();
            foreach (var condition in conditions) {
                groups.Add(condition.Group);
            }
            return groups.Count;
        }

        private PromotionArticleContainer UpdatePromotionArticleData(PromotionArticleContainer container, List<int> articleIDs, bool hasTagsCondition) {
            if (container == null) {
                container = new PromotionArticleContainer();
            }
            // if the current container has no tag info, and there are promotions with tags, fetch them:
            var existingArticleIDs = !container.HasTagInfo && hasTagsCondition ? new List<int>() : container.ArticleIDs;
            var articleIDsToFetch = articleIDs.Except(existingArticleIDs).Distinct().ToList();
            Dictionary<int, PromotionArticleWrapper> promotionArticleData = null;
            if (articleIDsToFetch.HasAny()) {
                promotionArticleData = GetNewArticleData(articleIDsToFetch, container.HasTagInfo || hasTagsCondition);
            }
            container.UpdateData(promotionArticleData, container.HasTagInfo || hasTagsCondition);
            return container;
        }

        protected abstract List<PromotionWrapper> GetValidPromotions(PromotionRecognitionParameters parameters);

        protected abstract Dictionary<int, PromotionArticleWrapper> GetNewArticleData(List<int> articleIDs, bool includeTags);
    }
}