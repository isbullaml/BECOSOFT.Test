using System;
using System.Diagnostics;

namespace BECOSOFT.Data.Migrator {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class MigrationResult<T> where T : Enum {
        public T Type { get; }
        public int Original { get; set; }
        public int Current { get; set; }
        public bool IsSuccessful { get; set; }

        public MigrationResult(T type) {
            Type = type;
        }

        /// <inheritdoc />
        public override string ToString() {
            return $"{IsSuccessful} migrating from {Original} to {Current}";
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [DebuggerHidden]
        private string DebuggerDisplay => ToString();
    }
}