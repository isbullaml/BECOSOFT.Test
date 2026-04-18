using BECOSOFT.Data.Migrator.Services.Interfaces;
using BECOSOFT.Data.Services.Interfaces;
using BECOSOFT.Utilities.Extensions.Collections;
using System;

namespace BECOSOFT.Data.Migrator.Services {
    /// <inheritdoc />
    public abstract class BaseMigrationService<T> : IBaseMigrationService<T> where T : Enum {
        private IProgressService _progressService;
        private readonly Lazy<IDatabaseMigratorFactory<T>> _migratorFactory;
        private readonly IBaseMigrationInformationProvider<T> _informationProvider;

        protected BaseMigrationService(Lazy<IDatabaseMigratorFactory<T>> migratorFactory, IBaseMigrationInformationProvider<T> informationProvider) {
            _migratorFactory = migratorFactory;
            _informationProvider = informationProvider;
        }

        /// <inheritdoc />
        public void SetProgressService(IProgressService progressService) {
            _progressService = progressService;
        }

        /// <inheritdoc />
        public MigrationResult<T> Upgrade(T type) {
            return Upgrade(type, withReset: false, performExtraCache: true);
        }

        /// <inheritdoc />
        public MigrationResult<T> UpgradeWithReset(T type) {
            return Upgrade(type, withReset: true, performExtraCache: true);
        }

        /// <inheritdoc />
        public bool HasType(T type) {
            var migrator = _migratorFactory.Value.CreateMigrator();
            migrator.Load(type);
            return migrator.Current != null;
        }

        private MigrationResult<T> Upgrade(T type, bool withReset, bool performExtraCache, bool fromLinked = false) {
            var migrator = _migratorFactory.Value.CreateMigrator();
            migrator.SetFromLinked(fromLinked);
            BindEvent(migrator);
            migrator.Load(type);
            var originalVersion = migrator.Current.Version;
            if (withReset) {
                migrator.ResetVersion(type);
            }

            migrator.MigrateToLatest();
            var current = migrator.Current.Version;
            var result = new MigrationResult<T>(type) {
                Original = originalVersion,
                Current = current,
                IsSuccessful = migrator.LatestMigration.Version == current
            };
            var linkedMigrationTypesToRun = _informationProvider.LinkedMigrationTypes?.TryGetValueWithDefault(type);
            if (linkedMigrationTypesToRun.HasAny()) {
                foreach (var migrationTypeToRun in linkedMigrationTypesToRun) {
                    Upgrade(migrationTypeToRun, withReset, false, fromLinked: true);
                }
            }

            if (performExtraCache) {
                PerformExtraCache(type);
            }

            return result;
        }

        protected virtual void PerformExtraCache(T type) {
        }

        internal void BindEvent(IMigrator<T> migrator) {
            if (_progressService != null) {
                migrator.ProgressHandler += (current, total, description) => {
                    _progressService.Set(current / total, description);
                };
            }
        }
    }
}