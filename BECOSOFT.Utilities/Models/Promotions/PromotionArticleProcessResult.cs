using BECOSOFT.Utilities.Extensions.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace BECOSOFT.Utilities.Models.Promotions {
    /// <summary>
    /// This object contains the promotion processing result for a given index.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class PromotionArticleProcessResult {

        /// <summary>
        /// Returns the <see cref="Index"/> property of <see cref="OriginalArticleData"/>
        /// </summary>
        public int Index => OriginalArticleData.Index;

        /// <summary>
        /// Returns the <see cref="ArticleID"/> property of <see cref="OriginalArticleData"/>
        /// </summary>
        public int ArticleID => OriginalArticleData.ArticleID;

        /// <summary>
        /// Contains the original <see cref="ArticleData"/> that was passed to the process function.
        /// </summary>
        public ArticleData OriginalArticleData { get; }
        /// <summary>
        /// A list of <see cref="ArticleDataAdjustment"/> objects, containing the adjustments that need to be processed on <see cref="OriginalArticleData"/>.
        /// </summary>
        public List<ArticleDataAdjustment>  ArticleDataAdjustments { get; set; } = new List<ArticleDataAdjustment>();
        /// <summary>
        /// This <see cref="ArticleData"/> property contains the remaining article quantity that is not used by a promotion.
        /// </summary>
        public ArticleData NewArticleData { get; set; }

        public bool IsUntouched => NewArticleData == null && ArticleDataAdjustments.IsEmpty();

        /// <summary>
        /// Construct a new <see cref="PromotionArticleProcessResult"/> from an <see cref="ArticleData"/> object.
        /// </summary>
        /// <param name="originalArticleData"></param>
        public PromotionArticleProcessResult(ArticleData originalArticleData) {
            OriginalArticleData = originalArticleData;
        }

        /// <summary>
        /// Perform the <paramref name="adjustment"/> on a copy of the <see cref="OriginalArticleData"/> object.
        /// </summary>
        /// <param name="adjustment"><see cref="ArticleDataAdjustment"/> to perform</param>
        /// <returns></returns>
        public ArticleData PerformAdjustment(ArticleDataAdjustment adjustment) {
            var copy = OriginalArticleData.Clone();
            copy.Quantity = adjustment.Quantity;
            copy.PromotionID = adjustment.PromotionID;
            if (adjustment.HasAdjustment) {
                if (copy.HasPromotionPrice) {
                    copy.BasePrice = copy.PriceWithoutPromotion ?? copy.BasePrice;
                    copy.PriceWithoutPromotion = null;
                }
                if (adjustment.BasePrice.HasValue) { copy.BasePrice = adjustment.BasePrice.Value; }
                if (adjustment.Discount1.HasValue) { copy.Discount1 = adjustment.Discount1.Value; }
                if (adjustment.Discount2.HasValue) { copy.Discount2 = adjustment.Discount2.Value; }
                if (adjustment.Discount3.HasValue) { copy.Discount3 = adjustment.Discount3.Value; }
                if (adjustment.Discount4.HasValue) { copy.Discount4 = adjustment.Discount4.Value; }
                if (adjustment.Discount5.HasValue) { copy.Discount5 = adjustment.Discount5.Value; }
                if (adjustment.DiscountAmount.HasValue) { copy.DiscountAmount = adjustment.DiscountAmount.Value; }
                if (!copy.IsNoPoints && adjustment.PointFactor.HasValue) { copy.PointFactor = adjustment.PointFactor.Value; }
            }
            return copy;
        }

        private string DebuggerDisplay => $"Index: {Index}, ArtID:: {OriginalArticleData.ArticleID} x {OriginalArticleData.Quantity} - IsUntouched? {IsUntouched}";
    }
}