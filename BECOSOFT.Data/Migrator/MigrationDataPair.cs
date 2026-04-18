using System;
using System.Diagnostics;

namespace BECOSOFT.Data.Migrator {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal class MigrationDataPair<T> where T : Enum {
        public MigrationData<T> To { get; }

        public MigrationData<T> From { get; }

        public MigrationDataPair(MigrationData<T> from, MigrationData<T> to) {
            From = from;
            To = to;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [DebuggerHidden]
        private string DebuggerDisplay => $"{From.Type} ({From.Version}) -> {To.Type} ({To.Version})";
    }
}