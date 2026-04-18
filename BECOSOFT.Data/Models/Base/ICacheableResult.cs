using BECOSOFT.Data.Caching.Interfaces;
using BECOSOFT.Data.Repositories;

namespace BECOSOFT.Data.Models.Base {
    /// <summary>
    /// This interface defines that the property  <see cref="ICacheable.IsCachingPossible"/> on <see cref="BaseResultRepository{T}"/> should be enabled.
    /// </summary>
    public interface ICacheableResult { }
}