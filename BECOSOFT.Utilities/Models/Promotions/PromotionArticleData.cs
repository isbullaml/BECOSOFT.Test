using System;
using System.Diagnostics;

namespace BECOSOFT.Utilities.Models.Promotions {
    /// <summary>
    /// This object is the singular (quantity &lt;= 1) form of the corresponding <see cref="ArticleData"/> object (by index). Each <see cref="PromotionArticleData"/> gets a unique <see cref="ID"/> in the calculations.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class PromotionArticleData : IEquatable<PromotionArticleData> {
        /// <summary>
        /// Unique ID
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// ArticleID of the original <see cref="ArticleData"/> object
        /// </summary>
        public int ArticleID { get; }

        /// <summary>
        /// Index of the original <see cref="ArticleData"/> object
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Quantity, less than or equal to 1
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Price of the original <see cref="ArticleData"/> object
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Purchase price of the original <see cref="ArticleData"/> object
        /// </summary>
        public decimal? PurchasePrice { get; set; }

        /// <summary>
        /// Weight of the article
        /// </summary>
        public decimal Weight { get; set; }

        /// <summary>
        /// Indicates that the <see cref="Quantity"/> from the product is from weighing.
        /// </summary>
        public bool IsWeighted { get; set; }

        /// <summary>
        /// Article information, required for <see cref="PromotionConditionWrapper"/> calculations
        /// </summary>
        public PromotionArticleWrapper Data { get; set; }

        public PromotionArticleData(int id, int articleID, int index) {
            ID = id;
            ArticleID = articleID;
            Index = index;
        }

        /// <inheritdoc/>
        public bool Equals(PromotionArticleData other) {
            if (ReferenceEquals(null, other)) { return false; }
            if (ReferenceEquals(this, other)) { return true; }
            return ID == other.ID;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            if (obj.GetType() != GetType()) { return false; }
            return Equals((PromotionArticleData)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode() {
            return ID.GetHashCode();
        }

        private string DebuggerDisplay => $"ID: {ID}, Index: {Index} ({Quantity}x Art: {ArticleID})";

        public override string ToString() {
            return DebuggerDisplay;
        }
    }
}