using System;
using System.Data;

namespace BECOSOFT.Data.Optimizer {
    public interface IOptimization<out T> where T : Enum {
        T OptimizationType { get; }
        void RunOptimization(IDbConnection connection, bool logIndexStatistics);
    }
}