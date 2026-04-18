using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;

namespace BECOSOFT.Data.Models.QueryData {
    public class ReplicatedTableResult {
        private readonly List<ReplicatedTableEntry> _replicatedTables;
        private readonly List<ReplicatedTableEntry> _missingTables;
        private readonly List<ReplicatedTableEntry> _uncheckedTables;
        public IReadOnlyList<ReplicatedTableEntry> ReplicatedTables => _replicatedTables.AsReadOnly();
        public IReadOnlyList<ReplicatedTableEntry> MissingTables => _missingTables.AsReadOnly();
        public IReadOnlyList<ReplicatedTableEntry> UncheckedTables => _uncheckedTables.AsReadOnly();

        public bool NoReplicationDefined { get; set; }

        public bool HasMissing => MissingTables.HasAny();

        public ReplicatedTableResult() : this(Array.Empty<ReplicatedTableEntry>()) {
        }

        public ReplicatedTableResult(IEnumerable<ReplicatedTableEntry> entries) {
            _replicatedTables = new List<ReplicatedTableEntry>();
            _missingTables = new List<ReplicatedTableEntry>(0);
            _uncheckedTables = new List<ReplicatedTableEntry>(0);

            foreach (var entry in entries) {
                if (entry.HasMissing) {
                    _missingTables.Add(entry);
                } else if (!entry.DidCheck) {
                    _uncheckedTables.Add(entry);
                } else {
                    _replicatedTables.Add(entry);
                }
            }
        }
    }
}