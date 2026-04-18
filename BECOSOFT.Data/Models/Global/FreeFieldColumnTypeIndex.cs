using System;
using System.Diagnostics;

namespace BECOSOFT.Data.Models.Global {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly struct FreeFieldColumnTypeIndex : IEquatable<FreeFieldColumnTypeIndex> {
        public FreeFieldColumnType ColumnType { get; }
        public int Index { get; }

        public FreeFieldColumnTypeIndex(FreeFieldColumnType columnType, int index) {
            ColumnType = columnType;
            Index = index;
        }

        public bool Equals(FreeFieldColumnTypeIndex other) {
            return ColumnType == other.ColumnType && Index == other.Index;
        }

        public override bool Equals(object obj) {
            return obj is FreeFieldColumnTypeIndex other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked { return ((int)ColumnType * 397) ^ Index; }
        }

        public static bool operator ==(FreeFieldColumnTypeIndex left, FreeFieldColumnTypeIndex right) {
            return left.Equals(right);
        }

        public static bool operator !=(FreeFieldColumnTypeIndex left, FreeFieldColumnTypeIndex right) {
            return !left.Equals(right);
        }

        private string DebuggerDisplay => $"{ColumnType}, {Index}";
    }
}