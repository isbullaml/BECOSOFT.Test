using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace BECOSOFT.Data.Migrator {
    /// <inheritdoc />
    internal class DatabaseMigrator<T> : IMigrator<T> where T: Enum {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IMigrationFinder<T> _finder;
        private readonly IDatabaseProvider<T> _provider;
        private readonly IBaseMigrationInformationProvider<T> _informationProvider;
        private readonly IEnumerable<IBaseMigrationInjector<T>> _injectors;

        private IReadOnlyList<MigrationData<T>> _migrations;
        private bool _isLoaded;
        private bool _fromLinked;

        /// <inheritdoc />
        public event MigrationEventHandler ProgressHandler;

        /// <inheritdoc />
        public MigrationData<T> Current { get; set; }

        private T _type;

        /// <inheritdoc />
        public MigrationData<T> LatestMigration => _migrations.IsEmpty() ? null : _migrations.Last();

        public DatabaseMigrator(IMigrationFinder<T> finder, IDatabaseProvider<T> provider,
                                IBaseMigrationInformationProvider<T> informationProvider,
                                IEnumerable<IBaseMigrationInjector<T>> injectors) {
            _finder = finder;
            _provider = provider;
            _informationProvider = informationProvider;
            _injectors = injectors;
        }

        public void RaiseEvent(int currentVersion, int toVersion, string description) {
            ProgressHandler?.Invoke(currentVersion, toVersion, description);
        }

        private void EnsureLoaded() {
            if (!_isLoaded) {
                throw new InvalidOperationException("Not loaded yet");
            }
        }

        /// <inheritdoc />
        public void Load(T type) {
            if (!_isLoaded) {
                _type = type;
                FindAndSetMigrations();
            }
            if (_provider == null) { return; }

            var currentVersion = _provider.EnsurePrerequisitesCreatedAndGetCurrentVersion(_type);
            SetCurrentVersion(currentVersion, false);
            _isLoaded = true;
        }

        private void FindAndSetMigrations() {
            var migrations = _finder.FindMigrations(_type).ToList();

            Validate(migrations);

            _migrations = new[] { MigrationData<T>.BaseMigration() }.Concat(migrations.OrderBy(m => m.Version)).ToList();
        }

        private void Validate(IReadOnlyCollection<MigrationData<T>> migrations) {
            if (!migrations.HasAny()) {
                throw new MigrationException("The configured MigrationProvider did not find any migrations");
            }

            var migrationBaseTypeInfo = typeof(BaseMigration<>).GetTypeInfo();
            var firstNotImplementingTMigrationBase = migrations.FirstOrDefault(x => !x.TypeInfo.IsSubclassOfRawGeneric(migrationBaseTypeInfo));
            if (firstNotImplementingTMigrationBase != null) {
                throw new MigrationException($"Migration {firstNotImplementingTMigrationBase.TypeInfo.Name} must derive from / implement {nameof(BaseMigration<T>)}.");
            }

            var firstWithInvalidVersion = migrations.FirstOrDefault(x => x.Version <= 0);
            if (firstWithInvalidVersion != null) {
                throw new MigrationException($"Migration {firstWithInvalidVersion.TypeInfo.Name} must have a version > 0.");
            }

            var migrationClassType = _informationProvider.GetMigrationClassType(_type);
            var firstWithInvalidType = migrations.FirstOrDefault(x => !x.TypeInfo.BaseType.IsSameOrSubclassOf(migrationClassType));
            if (firstWithInvalidType != null) {
                throw new MigrationException($"Migration {firstWithInvalidType.TypeInfo.Name} has to be a {migrationClassType.Name}.");
            }

            var versionsPresent = migrations.Select(m => m.Version).ToList();
            var minVersion = versionsPresent.Min();
            var maxVersion = versionsPresent.Max();
            var range = Enumerable.Range(minVersion, maxVersion - minVersion);
            var missingVersion = range.Except(versionsPresent).ToList();
            if (missingVersion.HasAny()) {
                throw new MigrationException($"Missing following migrations with version: {string.Join(", ", missingVersion)}.");
            }
            var versionLookup = migrations.ToLookup(x => x.Version);
            var firstDuplicate = versionLookup.FirstOrDefault(x => x.Count() > 1);
            if (firstDuplicate != null) {
                throw new MigrationException($"Found more than one migration with version {firstDuplicate.Key} ({string.Join(", ", firstDuplicate)}).");
            }
        }

        /// <inheritdoc />
        public void MigrateTo(int version) {
            EnsureLoaded();

            try {
                var connection = _provider.Begin();

                var currentVersion = _provider.GetCurrentVersion(_type);
                SetCurrentVersion(currentVersion, false);

                var toMigration = _migrations.FirstOrDefault(m => m.Version == version);
                if (toMigration == null) {
                    throw new MigrationException();
                }

                var fromMigration = Current;

                if (_informationProvider.IsDynamic(_type)) {
                    var tableInfo = _migrations.FirstOrDefault(m => m.Instance != null)?.Instance.GetTableInfo();
                    if (tableInfo == null) {
                        throw new NotImplementedException();
                    }
                    var dynamicVersions = _provider.GetCurrentDynamicVersions(_type, tableInfo);
                    if (dynamicVersions.HasAny()) {
                        var minDynamicVersion = dynamicVersions.OrderBy(d => d.Value).FirstOrDefault().Value;
                        SetCurrentVersion(minDynamicVersion, false);
                    }
                }
                var migrations = FindMigrationsToRun(version).ToList();
                SetCurrentVersion(currentVersion, false);

                if (migrations.IsEmpty()) {
                    return;
                }
                var injectors = _injectors.ToSafeList();
                Logger.Info($"Starting migration sequence for migration type '{_type}'. Version {fromMigration.Version} to {toMigration.Version}");
                try {
                    foreach (var migrationDataPair in migrations) {
                        var migrationToRun = migrationDataPair.To;
                        foreach (var injector in injectors) {
                            injector.Inject(migrationToRun);
                        }

                        RaiseEvent(migrationToRun.Version, toMigration.Version, migrationToRun.ToString());
                        try {
                            Logger.Info($"Begin migration \"{migrationToRun.Description}\", version {migrationToRun.Version}.");
                            RunMigration(migrationToRun, connection);
                            _provider.UpdateVersion(migrationDataPair.From.Version, migrationToRun.Version, migrationToRun.Description, _type);
                            if (_informationProvider.IsDynamic(_type)) {
                                _provider.UpdateDynamicVersion(migrationToRun.Version, _type, migrationToRun.DynamicEntityIDs);
                            }
                            Logger.Info($"Ended migration \"{migrationToRun.Description}\", version {migrationToRun.Version}.");
                        } catch (Exception e) {
                            Logger.Error(e, $"Ended migration with error. \"{migrationToRun.Description}\", version {migrationToRun.Version} failed.");
                            Logger.Error(e);
                            throw;
                        }
                    }
                } catch (Exception e) {
                    Logger.Error(e, $"Ended migration sequence for migration type '{_type}' with error. Version {fromMigration.Version} to {toMigration.Version}.");
                    throw;
                } finally {
                    var finalVersion = _provider.GetCurrentVersion(_type);
                    SetCurrentVersion(finalVersion, true);
                }

            } finally {
                _provider.End();
            }
        }

        /// <inheritdoc />
        public void ResetVersion(T type) {
            var currentVersion = _provider.EnsurePrerequisitesCreatedAndGetCurrentVersion(type);
            _provider.UpdateVersion(currentVersion, 0, "Reset", type);
        }

        public void SetFromLinked(bool fromLinked) {
            _fromLinked = fromLinked;
        }

        private void SetCurrentVersion(int currentVersion, bool onMigrationFinished) {
            var lastAvailable = LatestMigration.Version;
            if (!_informationProvider.ShouldRegisterVersion(_type) && onMigrationFinished) {
                currentVersion = lastAvailable;
            }

            var current = lastAvailable < currentVersion ? lastAvailable : currentVersion;
            var currentMigration = _migrations.FirstOrDefault(m => m.Version == current);
            Current = currentMigration;
        }

        private void RunMigration(MigrationData<T> migrationData, IDbConnection connection) {
            var migration = CreateMigration(migrationData);
            var data = new MigrationRunData(connection, Logger);
            migration.RunUpgrade(data);
        }

        private IMigration<T> CreateMigration(MigrationData<T> migrationData) {
            if (migrationData == null) {
                throw new ArgumentNullException(nameof(migrationData));
            }

            IMigration<T> instance;
            try {
                instance = migrationData.Instance;
                instance.SetRunningFromLinked(_fromLinked);
            } catch (Exception e) {
                throw new MigrationException("Unable to create migration", e);
            }

            return instance;
        }

        private IEnumerable<MigrationDataPair<T>> FindMigrationsToRun(int version) {
            for (var i = 0; i < _migrations.Count; i++) {
                var migration = _migrations[i];
                if (migration.Version > Current.Version && migration.Version <= version) {
                    yield return new MigrationDataPair<T>(_migrations[i - 1], migration);
                }
            }
        }

        /// <inheritdoc />
        public void MigrateToLatest() {
            MigrateTo(LatestMigration.Version);
        }

        public void Dispose() {

        }
    }
}