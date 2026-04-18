using BECOSOFT.Utilities.Extensions.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Models.Promotions {
    public class MultiPromotionProcessResult {
        public bool DidProcessPromotions => Results.Any(r => r.DidProcessPromotions);
        public List<PromotionProcessResult> Results { get; set; } = new List<PromotionProcessResult>();
    }
    /// <summary>
    /// This class contains the result of the promotion calculation.
    /// </summary>
    public class PromotionProcessResult {
        /// <summary>
        ///  The <see cref="PromotionParameters"/> that were used to get the result
        /// </summary>
        public PromotionParameters OriginalParameters { get; }
        /// <summary>
        /// Contains all Promotion IDs (and how many times the promotion conditions have been met) that have been activated because of a promotion with Activation conditions. 
        /// </summary>
        public List<ActivatedPromotionResult> ActivatedPromotions { get; set; } = new List<ActivatedPromotionResult>();

        /// <summary>
        /// A <see cref="HashSet{T}"/> of promotions IDs that have been activated by a voucher and had their action processed on the article data.
        /// </summary>
        public HashSet<int> ActivatedPromotionIDsInResult { get; set; } = new HashSet<int>();
        /// <summary>
        /// This <see cref="Dictionary{TKey,TValue}"/> contains the <see cref="PromotionArticleProcessResult"/> per index.
        /// </summary>
        public Dictionary<int, PromotionArticleProcessResult> ArticleResultPerIndex { get; set; } = new Dictionary<int, PromotionArticleProcessResult>();
        /// <summary>
        /// Contains the action <see cref="PromotionActionType.AddArticleWithPrice"/> result
        /// </summary>
        public List<PromotionAddArticleWithPriceAction> ArticlesWithPrice { get; set; } = new List<PromotionAddArticleWithPriceAction>(0);
        /// <summary>
        /// Contains the action <see cref="PromotionActionType.AddArticleWithDiscount"/> result
        /// </summary>
        public List<PromotionAddArticleWithDiscountAction> ArticlesWithDiscount { get; set; } = new List<PromotionAddArticleWithDiscountAction>(0);
        /// <summary>
        /// Contains the action <see cref="PromotionActionType.Voucher"/> (with percentage discount) result
        /// </summary>
        public List<PromotionAddVoucherWithDiscountAction> VoucherWithDiscount { get; set; } = new List<PromotionAddVoucherWithDiscountAction>(0);
        /// <summary>
        /// Contains the action <see cref="PromotionActionType.Voucher"/> (with value discount) result
        /// </summary>
        public List<PromotionAddVoucherWithDiscountValueAction> VoucherWithDiscountValue { get; set; } = new List<PromotionAddVoucherWithDiscountValueAction>(0);
        /// <summary>
        /// Contains a list of articles to remove (index based).
        /// </summary>
        public List<ArticleData> ArticlesToRemove { get; set; } = new List<ArticleData>(0);
        

        /// <summary>
        /// Contains a list of <see cref="PromotionInfo"/>, promotions that have been applied to the provided <see cref="OriginalParameters"/>
        /// </summary>
        public List<PromotionInfo> PromotionNames { get; set; } = new List<PromotionInfo>(0);

        public bool DidProcessPromotions { get; set; } = true;

        public PromotionProcessResult(PromotionParameters originalParameters) {
            OriginalParameters = originalParameters;
        }

        internal PromotionProcessResult() {
        }

        public void Merge(PromotionProcessResult tempResult) {
            DidProcessPromotions |= tempResult.DidProcessPromotions;
            ActivatedPromotions.AddRange(tempResult.ActivatedPromotions);
            var keys = ArticleResultPerIndex.Keys.Union(tempResult.ArticleResultPerIndex.Keys).ToDistinctList();
            var result = new Dictionary<int, PromotionArticleProcessResult>(keys.Count);
            foreach (var key in keys) {
                var processResult = ArticleResultPerIndex.TryGetValueWithDefault(key);
                var tempProcessResult = tempResult.ArticleResultPerIndex.TryGetValueWithDefault(key);
                if (processResult == null) {
                    result[key] = tempProcessResult;
                }else if (tempProcessResult == null) {
                    result[key] = processResult;
                } else {
                    result[key] = processResult;
                    processResult.ArticleDataAdjustments.AddRange(tempProcessResult.ArticleDataAdjustments);
                    if (processResult.NewArticleData != null && tempProcessResult.NewArticleData != null) {
                        processResult.NewArticleData.Quantity += tempProcessResult.NewArticleData.Quantity;
                    }
                }
            }
            ArticleResultPerIndex.Clear();
            ArticleResultPerIndex.AddRange(result);
            ActivatedPromotionIDsInResult.AddRange(tempResult.ActivatedPromotionIDsInResult);
            ArticlesWithPrice.AddRange(tempResult.ArticlesWithPrice);
            ArticlesWithDiscount.AddRange(tempResult.ArticlesWithDiscount);
            VoucherWithDiscount.AddRange(tempResult.VoucherWithDiscount);
            VoucherWithDiscountValue.AddRange(tempResult.VoucherWithDiscountValue);
            ArticlesToRemove.AddRange(tempResult.ArticlesToRemove);
            PromotionNames.AddRange(tempResult.PromotionNames);
        }
    }
}