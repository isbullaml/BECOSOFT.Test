using BECOSOFT.Data.Attributes;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BECOSOFT.Data.Models.QueryData {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ReplicatedTableEntry {
        public Type Type { get; set; }
        public TableDefinition TableDefinition { get; set; }
        public bool IsOptional { get; set; }
        public bool DidCheck { get; set; }

        public List<ReplicatedTableQueryResult> QueryResults { get; set; }

        public bool HasMissing => QueryResults != null && QueryResults.Any(qr => qr.IsMissing ?? false);
        private string DebuggerDisplay => $"{Type.Name}, Has missing? {HasMissing}, Checked? {DidCheck}, Optional? {IsOptional}, {QueryResults?.Count ?? 0} query results.";
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [ResultTable]
    public class ReplicatedTableQueryResult : BaseResult {
        [Column]
        public string TableName { get; set; }

        [Column]
        public bool? IsMissing { get; set; }

        [Column]
        public bool? IsReplicated { get; set; }

        public string TablePart { get; set; }

        private string DebuggerDisplay => $"{TableName}{(TablePart.HasNonWhiteSpaceValue() ? $" (Part: {TablePart})" : "")}, Missing? {IsMissing}, Replicated? {IsReplicated}";
    }
}