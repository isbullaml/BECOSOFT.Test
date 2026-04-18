using System;

namespace BECOSOFT.Utilities.Models.Promotions {
    public readonly struct ActivatedPromotionResult : IEquatable<ActivatedPromotionResult> {
        public int PromotionID { get; }
        public int NumberOfTimes { get; }
        public string Promotion { get; }
        public DateTime ValidFrom { get; }
        public DateTime ValidTo { get; }

        public ActivatedPromotionResult(int promotionID, int numberOfTimes, string promotion, 
                                        DateTime validFrom, DateTime validTo) {
            PromotionID = promotionID;
            NumberOfTimes = numberOfTimes;
            Promotion = promotion;
            ValidFrom = validFrom;
            ValidTo = validTo;
        }

        public bool Equals(ActivatedPromotionResult other) {
            return PromotionID == other.PromotionID && NumberOfTimes == other.NumberOfTimes && Promotion == other.Promotion && ValidFrom.Equals(other.ValidFrom) && ValidTo.Equals(other.ValidTo);
        }

        public override bool Equals(object obj) {
            return obj is ActivatedPromotionResult other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = PromotionID;
                hashCode = (hashCode * 397) ^ NumberOfTimes;
                hashCode = (hashCode * 397) ^ (Promotion != null ? Promotion.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ValidFrom.GetHashCode();
                hashCode = (hashCode * 397) ^ ValidTo.GetHashCode();
                return hashCode;
            }
        }
    }
}