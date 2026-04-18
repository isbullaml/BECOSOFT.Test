using BECOSOFT.Data.Migrator;
using System;
using System.Collections.Generic;

namespace BECOSOFT.Data.Optimizer {
    internal class DatabaseOptimizerFactory<T> : IDatabaseOptimizerFactory<T> where T:Enum {
        private readonly IEnumerable<IOptimization<T>> _optimizations;
        private readonly IAdvisoryLockProvider _provider;

        public DatabaseOptimizerFactory(IEnumerable<IOptimization<T>> optimizations, IAdvisoryLockProvider provider) {
            _optimizations = optimizations;
            _provider = provider;
        }

        public IOptimizer<T> CreateOptimizer() {
            return new DatabaseOptimizer<T>(_optimizations, _provider);
        }
    }
}