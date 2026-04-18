using System;
using System.Diagnostics;

namespace BECOSOFT.Utilities.Models.Promotions {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class PromotionRecognition : IEquatable<PromotionRecognition> {
        public int PromotionID { get; }
        public string Name { get; }

        public PromotionRecognition(int promotionID, string name) {
            PromotionID = promotionID;
            Name = name;
        }

        private string DebuggerDisplay => $"Promotion: {PromotionID}, '{Name}'";

        public bool Equals(PromotionRecognition other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }
            return PromotionID == other.PromotionID && Name == other.Name;
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
            return Equals((PromotionRecognition)obj);
        }

        public override int GetHashCode() {
            unchecked { return (PromotionID * 397) ^ (Name != null ? Name.GetHashCode() : 0); }
        }
    }
}