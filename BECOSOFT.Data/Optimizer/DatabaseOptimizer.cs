using BECOSOFT.Data.Migrator;
using BECOSOFT.Utilities.Extensions.Collections;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Data.Optimizer {
    internal class DatabaseOptimizer<T> : IOptimizer<T> where T : Enum {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly IEnumerable<IOptimization<T>> _optimizations;
        private readonly IAdvisoryLockProvider _provider;
        public event OptimizationEventHandler ProgressHandler;

        public DatabaseOptimizer(IEnumerable<IOptimization<T>> optimizations, IAdvisoryLockProvider provider) {
            _optimizations = optimizations;
            _provider = provider;
        }

        public void RaiseEvent(int currentIndex, int totalCount, string description) {
            ProgressHandler?.Invoke(currentIndex, totalCount, description);
        }

        public void Optimize(T type, bool logIndexStatistics) {
            try {
                var connection = _provider.Begin();
                var optimizations = GetOptimizations(type);

                if (optimizations.IsEmpty()) {
                    return;
                }
                var totalCount = optimizations.Count;
                Logger.Info($"Starting optimization sequence for optimization type '{type.ToString()}'. ({totalCount} optimizations)");
                try {
                    for (var i = 0; i < optimizations.Count; i++) {
                        var optimization = optimizations[i];
                        var description = optimization.GetType().Name;
                        RaiseEvent(i, totalCount, description);
                        try {
                            Logger.Info($"Begin optimization \"{description}\". ({i + 1}/{totalCount})");
                            optimization.RunOptimization(connection, logIndexStatistics);
                            Logger.Info($"Ended optimization \"{description}\". ({i + 1}/{totalCount})");
                        } catch (Exception e) {
                            Logger.Error(e, $"Ended optimization with error. \"{description}\". ({i + 1}/{totalCount})");
                            Logger.Error(e);
                            throw;
                        }
                    }
                } catch (Exception e) {
                    Logger.Error(e, $"Ended optimization sequence for optimization type '{type.ToString()}' with error.");
                    throw;
                }
            } catch (Exception e) {
                Logger.Error(e);
            } finally {
                Logger.Info($"Ended optimization sequence for optimization type '{type.ToString()}'.");
                _provider.End();
            }
        }

        private List<IOptimization<T>> GetOptimizations(T optimizationType) {
            var comparer = EqualityComparer<T>.Default;
            return _optimizations.Where(p => comparer.Equals(p.OptimizationType, optimizationType)).ToList();
        }
    }
}