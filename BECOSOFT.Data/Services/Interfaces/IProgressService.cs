namespace BECOSOFT.Data.Services.Interfaces {
    public delegate void UpdateEventHandler(int percentage, string description = "");
    /// <summary>
    /// Service used for handling progress on another service
    /// </summary>
    public interface IProgressService : IBaseService {
        /// <summary>
        /// The event handler on which the events should be invoked.
        /// </summary>
        event UpdateEventHandler ProgressHandler;
        /// <summary>
        /// Invoke an event
        /// </summary>
        /// <param name="progress">The completion percentage of the action</param>
        /// <param name="description">An optional description to describe the current action</param>
        void Set(int progress, string description = "");
    }
}