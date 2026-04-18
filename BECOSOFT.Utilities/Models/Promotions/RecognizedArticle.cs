using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BECOSOFT.Utilities.Models.Promotions {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class RecognizedArticle : IEquatable<RecognizedArticle> {
        public int ArticleID { get; }

        public List<PromotionRecognition> RecognizedPromotions { get; set; }

        public RecognizedArticle(int articleID) {
            ArticleID = articleID;
            RecognizedPromotions = new List<PromotionRecognition>();
        }

        public bool Equals(RecognizedArticle other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }
            return ArticleID == other.ArticleID;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            if (ReferenceEquals(this, obj)) {
                return true;
            }
            if (obj.GetType() != GetType()) {
                return false;
            }
            return Equals((RecognizedArticle) obj);
        }

        public override int GetHashCode() {
            return ArticleID;
        }

        private string DebuggerDisplay => $"Article: {ArticleID}, {RecognizedPromotions.Count} promotions";
    }
}