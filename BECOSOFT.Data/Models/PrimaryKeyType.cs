using System;

namespace BECOSOFT.Data.Models {
    public class PrimaryKeyType : IEquatable<PrimaryKeyType> {
        public Type Type { get; }
        public string TablePart { get; }

        public PrimaryKeyType(Type type, string tablePart) {
            Type = type;
            TablePart = tablePart;
        }

        public bool Equals(PrimaryKeyType other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }
            return Type == other.Type && TablePart == other.TablePart;
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
            return Equals((PrimaryKeyType)obj);
        }

        public override int GetHashCode() {
            unchecked { return ((Type != null ? Type.GetHashCode() : 0) * 397) ^ (TablePart != null ? TablePart.GetHashCode() : 0); }
        }

        public static bool operator ==(PrimaryKeyType left, PrimaryKeyType right) {
            return Equals(left, right);
        }

        public static bool operator !=(PrimaryKeyType left, PrimaryKeyType right) {
            return !Equals(left, right);
        }
    }
}