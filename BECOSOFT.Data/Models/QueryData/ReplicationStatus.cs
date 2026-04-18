namespace BECOSOFT.Data.Models.QueryData {
    public enum ReplicationStatus {
        Unknown = 0,
        Start = 1,
        Succeed = 2,
        InProgress = 3,
        Idle = 4,
        Retry = 5,
        Fail = 6,
    }
}