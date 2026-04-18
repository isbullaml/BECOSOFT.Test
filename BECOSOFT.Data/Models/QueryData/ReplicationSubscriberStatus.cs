using BECOSOFT.Data.Attributes;
using BECOSOFT.Data.Models.Base;
using System;
using System.Diagnostics;

namespace BECOSOFT.Data.Models.QueryData {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [ResultTable]
    public class ReplicationSubscriberStatus : BaseResult {
        [Column]
        public ReplicationStatus Status { get; set; }

        [Column]
        public string SubscriberDatabase { get; set; }

        [Column]
        public string Publication { get; set; }

        [Column]
        public string Subscriber { get; set; }

        [Column]
        public DateTime LastSynchronised { get; set; }

        [Column]
        public long UndistributedCommands { get; set; }

        [Column]
        public string Comments { get; set; }

        [Column]
        public string PublisherDatabase { get; set; }

        [Column]
        public ReplicationSubscriptionType SubscriptionType { get; set; }

        private string DebuggerDisplay => $"{Subscriber} {Status} ({Publication} on {PublisherDatabase}) Comments: {Comments}";
    }
}