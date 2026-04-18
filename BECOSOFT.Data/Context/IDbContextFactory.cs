using BECOSOFT.Data.Models.Base;

namespace BECOSOFT.Data.Context {
    /// <summary>
    /// Context factory
    /// </summary>
    public interface IDbContextFactory {
        string Connection { get; }

        /// <summary>
        /// Retrieve a context for <see cref="BaseEntity"/>
        /// </summary>
        /// <returns>The context</returns>
        IBaseEntityDbContext CreateBaseEntityContext();

        /// <summary>
        /// Retrieve a context
        /// </summary>
        /// <returns>The context</returns>
        IBaseResultDbContext CreateBaseResultContext();
    }
}
