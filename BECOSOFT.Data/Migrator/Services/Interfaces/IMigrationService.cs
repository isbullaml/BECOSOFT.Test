using BECOSOFT.Data.Services.Interfaces;
using System;

namespace BECOSOFT.Data.Migrator.Services.Interfaces {
    /// <summary>
    /// Service used for database-migrations
    /// </summary>
    public interface IBaseMigrationService<T> : IBaseService where T : Enum {
        /// <summary>
        /// Upgrade the database to the latest version
        /// of the given <see cref="T"/>
        /// </summary>
        /// <param name="type">The type of the migration</param>
        /// <returns>The result of the migration</returns>
        MigrationResult<T> Upgrade(T type);

        /// <summary>
        /// Upgrade the database to the latest version
        /// of the given <see cref="T"/> after setting it's version to 0
        /// </summary>
        /// <param name="type">The type of the migration</param>
        /// <returns>The result of the migration</returns>
        MigrationResult<T> UpgradeWithReset(T type);

        /// <summary>
        /// Checks whether the <see cref="T"/> exists in the version table
        /// </summary>
        /// <param name="type">Type of the migration</param>
        /// <returns></returns>
        bool HasType(T type);

        /// <summary>
        /// Sets the <see cref="IProgressService"/> of the current service,
        /// so the progress-events can be used
        /// </summary>
        /// <param name="progressService">The ProgressService to use</param>
        void SetProgressService(IProgressService progressService);
    }
}