using BECOSOFT.Data.Services.Interfaces;

namespace BECOSOFT.Data.Services {
    /// <inheritdoc />
    public class ProgressService : IProgressService {
        /// <inheritdoc />
        public event UpdateEventHandler ProgressHandler;

        /// <inheritdoc />
        public void Set(int progress, string description = "") {
            ProgressHandler?.Invoke(progress, description);
        }
    }

    /// <summary>
    /// Extension-methods for the <see cref="IProgressService"/>
    /// </summary>
    public static class ProgressServiceExtensions {
        /// <summary>
        /// Invoke an event
        /// </summary>
        /// <param name="progressService">The <see cref="IProgressService"/> on which the event needs to be invoked</param>
        /// <param name="percentage">The completion percentage of the action</param>
        /// <param name="description">An optional description to describe the current action</param>
        public static void Update(this IProgressService progressService, int percentage, string description = "") {
            progressService?.Set(percentage, description);
        }

        /// <summary>
        /// Reset the progress to 0
        /// </summary>
        /// <param name="progressService">The <see cref="IProgressService"/> on which the reset needs to be invoked</param>
        public static void Reset(this IProgressService progressService) {
            progressService?.Set(0);
        }
    }
}