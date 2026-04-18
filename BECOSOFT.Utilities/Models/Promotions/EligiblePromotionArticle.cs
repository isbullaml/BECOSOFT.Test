namespace BECOSOFT.Utilities.Models.Promotions {
    internal class EligiblePromotionArticle {
        public short PromotionGroupingIndex { get; set; }
        public PromotionArticleData ArticleData { get; set; }

        public string DebuggerDisplay => $"Grouping index: {PromotionGroupingIndex}, Data: {ArticleData}";
    }
}