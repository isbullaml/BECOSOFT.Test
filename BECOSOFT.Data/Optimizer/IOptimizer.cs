using System;

namespace BECOSOFT.Data.Optimizer {
    /// <summary>
    /// Event handler for optimization events
    /// </summary>
    /// <param name="currentIndex">The index of the current optimization</param>
    /// <param name="totalCount">The total count of optimizations</param>
    public delegate void OptimizationEventHandler(int currentIndex, int totalCount, string description);
    public interface IOptimizer<in T> where T : Enum {
        /// <summary>
        /// Event handler for optimization events
        /// </summary>
        event OptimizationEventHandler ProgressHandler;
        /// <summary>
        /// Optimize the databases
        /// </summary>
        void Optimize(T type, bool logIndexStatistics);
    }
}