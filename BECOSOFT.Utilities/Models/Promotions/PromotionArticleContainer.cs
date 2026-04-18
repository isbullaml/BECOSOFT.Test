using BECOSOFT.Utilities.Extensions.Collections;
using System.Collections.Generic;

namespace BECOSOFT.Utilities.Models.Promotions {
    public class PromotionArticleContainer {
        private readonly Dictionary<int, PromotionArticleWrapper> _promotionArticles;
        public bool HasTagInfo { get; private set; }
        public List<int> ArticleIDs => _promotionArticles.Keys.ToSafeList();
        public PromotionArticleContainer() {
            _promotionArticles = new Dictionary<int, PromotionArticleWrapper>();
        }
        public PromotionArticleContainer(Dictionary<int, PromotionArticleWrapper> promotionArticles, bool hasTagInfo) {
            HasTagInfo = hasTagInfo;
            _promotionArticles = promotionArticles;
        }

        public PromotionArticleWrapper Get(int articleID) {
            return _promotionArticles.TryGetValueWithDefault(articleID);
        }

        public void UpdateData(Dictionary<int, PromotionArticleWrapper> promotionArticles, bool hasTagInfo) {
            if (!HasTagInfo && hasTagInfo) {
                HasTagInfo = true;
            }
            _promotionArticles.AddRange(promotionArticles);
        }
    }
}