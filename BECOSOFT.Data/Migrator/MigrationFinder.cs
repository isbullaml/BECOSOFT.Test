using BECOSOFT.Data.Migrator.Attributes;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BECOSOFT.Data.Migrator {
    internal class MigrationFinder<T> : IMigrationFinder<T> where T : Enum {
        private readonly IEnumerable<IMigration<T>> _migrations;
        private readonly IBaseMigrationInformationProvider<T> _informationProvider;

        public MigrationFinder(IEnumerable<IMigration<T>> migrations,
                               IBaseMigrationInformationProvider<T> informationProvider) {
            _migrations = migrations;
            _informationProvider = informationProvider;
        }

        public IEnumerable<MigrationData<T>> FindMigrations(T migrationType) {
            var migrations = new List<MigrationData<T>>();
            var attributeType = GetAttributeType(migrationType);
            foreach (var migration in _migrations) {
                var type = migration.Type;

                var attribute = (BaseMigrationAttribute)type.GetCustomAttribute(attributeType);
                if (attribute == null) {
                    continue;
                }

                BaseMigrationAttribute descriptionAttribute;
                if (_informationProvider.DescriptionAttributeType != null) {
                    descriptionAttribute = (BaseMigrationAttribute)type.GetCustomAttribute(_informationProvider.DescriptionAttributeType) ?? attribute;
                } else {
                    descriptionAttribute = attribute;
                }

                var data = new MigrationData<T>(attribute.GetType<T>(), attribute.Version, descriptionAttribute.Information, type.GetTypeInfo(), migration);
                migrations.Add(data);
            }
            if (!migrations.HasAny()) {
                throw new MigrationException("No migrations defined in the provided DefinedTypes or Assembly.");
            }
            return migrations;
        }

        private Type GetAttributeType(T migrationType) {
            var migrationAttributeTypes = _informationProvider.MigrationAttributes;
            var migrationAttribute = migrationAttributeTypes.Select(t => Tuple.Create((BaseMigrationAttribute)TypeActivator.CreateInstance(t), t))
                                                            .FirstOrDefault(t => t.Item1.GetType<T>().Equals(migrationType));
            if (migrationAttribute == null) {
                throw new ArgumentOutOfRangeException(nameof(migrationType), migrationType, null);
            }
            return migrationAttribute.Item2;
        }
    }
}