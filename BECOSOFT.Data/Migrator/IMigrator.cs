using System;

namespace BECOSOFT.Data.Migrator {
    /// <summary>
    /// Event handler for migration events
    /// </summary>
    /// <param name="currentVersion">The current migration version</param>
    /// <param name="toVersion">The last migration version</param>
    /// <param name="description">The description of the current migration</param>
    public delegate void MigrationEventHandler(int currentVersion, int toVersion, string description);
    /// <summary>
    /// Internal service used to migrate a database
    /// </summary>
    public interface IMigrator<T> : IDisposable where T : Enum {
        /// <summary>
        /// Event handler for migration events
        /// </summary>
        event MigrationEventHandler ProgressHandler;
        /// <summary>
        /// The current version of the database
        /// </summary>
        MigrationData<T> Current { get; set; }
        /// <summary>
        /// The latest version of the database
        /// </summary>
        MigrationData<T> LatestMigration { get; }

        /// <summary>
        /// Load the migrations of a <see cref="T"/>
        /// </summary>
        /// <param name="type">The type to load from</param>
        void Load(T type);
        /// <summary>
        /// Migrate the database to a specific version
        /// </summary>
        /// <param name="version">The version to migrate to</param>
        void MigrateTo(int version);
        /// <summary>
        /// Migrate to the latest database-version
        /// </summary>
        void MigrateToLatest();
        /// <summary>
        /// Set the version of a specific type to 0
        /// </summary>
        /// <param name="type">The type of the migration</param>
        void ResetVersion(T type);

        /// <summary>
        /// Indicates that this migration is initiated via a linked migration.
        /// </summary>
        /// <param name="fromLinked"></param>
        void SetFromLinked(bool fromLinked);
    }
}