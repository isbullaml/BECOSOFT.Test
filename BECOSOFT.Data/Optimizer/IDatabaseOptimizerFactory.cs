using System;

namespace BECOSOFT.Data.Optimizer {
    internal interface IDatabaseOptimizerFactory<in T> where T : Enum {
        IOptimizer<T> CreateOptimizer();
    }
}