using System;
using System.Collections.Generic;

namespace BECOSOFT.Data.Migrator {
    internal class DatabaseMigratorFactory<T> : IDatabaseMigratorFactory<T> where T:Enum {
        private readonly IMigrationFinder<T> _finder;
        private readonly IDatabaseProvider<T> _provider;
        private readonly IBaseMigrationInformationProvider<T> _informationProvider;
        private readonly IEnumerable<IBaseMigrationInjector<T>> _injectors;

        public DatabaseMigratorFactory(IMigrationFinder<T> finder, 
                                       IDatabaseProvider<T> provider, 
                                       IBaseMigrationInformationProvider<T> informationProvider,
                                       IEnumerable<IBaseMigrationInjector<T>> injectors) {
            _finder = finder;
            _provider = provider;
            _informationProvider = informationProvider;
            _injectors = injectors;
        }

        public IMigrator<T> CreateMigrator() {
            return new DatabaseMigrator<T>(_finder, _provider, _informationProvider, _injectors);
        }
    }
}