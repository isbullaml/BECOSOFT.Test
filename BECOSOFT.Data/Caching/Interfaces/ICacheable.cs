namespace BECOSOFT.Data.Caching.Interfaces {
    public interface ICacheable {
        bool IsCachingPossible { get; }
        bool IsCachingEnabled { get; set; }
    }
}