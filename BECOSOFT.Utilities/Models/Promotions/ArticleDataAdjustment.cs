using System;
using System.Text;

namespace BECOSOFT.Utilities.Models.Promotions {
    /// <summary>
    /// This class contains the adjustments that need to be made on the original <see cref="ArticleData"/> object.
    /// </summary>
    public class ArticleDataAdjustment : IEquatable<ArticleDataAdjustment> {
        public int PromotionID { get; set; }
        public short PromotionGroupingIndex { get; set; }
        public decimal Quantity { get; set; }
        public decimal? BasePrice { get; set; }
        public decimal? Discount1 { get; set; }
        public decimal? Discount2 { get; set; }
        public decimal? Discount3 { get; set; }
        public decimal? Discount4 { get; set; }
        public decimal? Discount5 { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? PointFactor { get; set; }

        /// <summary>
        /// Sets a discount <see cref="value"/> on the given <see cref="index"/>.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if an invalid <paramref name="index"/> is given (<paramref name="index"/> &lt; 1 or <paramref name="index"/> &gt; 5).</exception>
        public void SetDiscount(int index, decimal? value) {
            switch (index) {
                case 1:
                    Discount1 = value;
                    break;
                case 2:
                    Discount2 = value;
                    break;
                case 3:
                    Discount3 = value;
                    break;
                case 4:
                    Discount4 = value;
                    break;
                case 5:
                    Discount5 = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Returns whether this <see cref="ArticleDataAdjustment"/> contains an adjustment property with a value.
        /// </summary>
        public bool HasAdjustment => BasePrice.HasValue || Discount1.HasValue || Discount2.HasValue 
                                     || Discount3.HasValue || Discount4.HasValue || Discount5.HasValue 
                                     || DiscountAmount.HasValue || PointFactor.HasValue;

        public string GetAdjustmentString() {
            var sb = new StringBuilder();
            if (BasePrice.HasValue) {
                sb.AppendFormat("Base price: {0}", BasePrice.Value);
            }
            for (var i = 1; i <= 5; i++) {
                var disc = GetDiscount(i);
                if (!disc.HasValue) { continue; }
                if (sb.Length > 0) { sb.Append(", "); }
                sb.AppendFormat("Disc {0}: {1}", i, disc.Value);
            }
            if (DiscountAmount.HasValue) {
                if (sb.Length > 0) { sb.Append(", "); }
                sb.AppendFormat("Disc am: {0}", DiscountAmount.Value);
            }
            if (PointFactor.HasValue) {
                if (sb.Length > 0) { sb.Append(", "); }
                sb.AppendFormat("Point f: {0}", PointFactor.Value);
            }

            return sb.ToString();
        }

        /// <summary>
        /// If <see cref="IsMarkedArticle"/>, it means this article does not need an adjustment. It however requires to be marked as used by <see cref="PromotionID"/> for the <see cref="Quantity"/>.
        /// </summary>
        public bool IsMarkedArticle => !HasAdjustment;

        /// <summary>
        /// Indicates that the adjustment is coming from a <see cref="PromotionWrapper"/> with <see cref="PromotionWrapper.AlwaysExecute"/> set to <see langword="true"/>.
        /// If this is the only adjustment, it will allow promotion prices to be set instead of resetting the product to a non promotion price.
        /// </summary>
        public bool IsMarkedWithAlwaysExecutePromotion { get; set; }

        public bool Equals(ArticleDataAdjustment other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }
            return PromotionID == other.PromotionID 
                   && Quantity == other.Quantity && BasePrice == other.BasePrice 
                   && Discount1 == other.Discount1 && Discount2 == other.Discount2 
                   && Discount3 == other.Discount3 && Discount4 == other.Discount4 
                   && Discount5 == other.Discount5 && DiscountAmount == other.DiscountAmount 
                   && PointFactor == other.PointFactor 
                   && PromotionGroupingIndex == other.PromotionGroupingIndex;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            if (ReferenceEquals(this, obj)) {
                return true;
            }
            if (obj.GetType() != this.GetType()) {
                return false;
            }
            return Equals((ArticleDataAdjustment) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = PromotionID;
                hashCode = (hashCode * 397) ^ Quantity.GetHashCode();
                hashCode = (hashCode * 397) ^ BasePrice.GetHashCode();
                hashCode = (hashCode * 397) ^ Discount1.GetHashCode();
                hashCode = (hashCode * 397) ^ Discount2.GetHashCode();
                hashCode = (hashCode * 397) ^ Discount3.GetHashCode();
                hashCode = (hashCode * 397) ^ Discount4.GetHashCode();
                hashCode = (hashCode * 397) ^ Discount5.GetHashCode();
                hashCode = (hashCode * 397) ^ DiscountAmount.GetHashCode();
                hashCode = (hashCode * 397) ^ PointFactor.GetHashCode();
                hashCode = (hashCode * 397) ^ PromotionGroupingIndex.GetHashCode();
                return hashCode;
            }
        }
        public decimal? GetDiscount(int index) {
            switch (index) {
                case 1:
                    return Discount1;
                case 2:
                    return Discount2;
                case 3:
                    return Discount3;
                case 4:
                    return Discount4;
                case 5:
                    return Discount5;
                default:
                    return null;
            }
        }
    }
}