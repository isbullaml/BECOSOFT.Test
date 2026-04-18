using System;

namespace BECOSOFT.Utilities.IO.Scales {
    public abstract class Scale : IDisposable {
        public abstract ScaleType Type { get; }
        
        /// <summary>
        /// This event is raised each time a weight is read from the scale.
        /// If <see cref="ScaleWeightPolling"/> is <see cref="ScaleWeightPolling.Once"/>, this event will only be called once.
        /// </summary>
        public event EventHandler<ScaleEventArgs> WeightReceived;

        /// <summary>
        /// Indicates whether the scale is currently being polled.
        /// </summary>
        public abstract bool IsPolling { get; protected set; }

        /// <summary>
        /// Start weight polling using te specified <see cref="ScaleWeightPolling"/> value.
        /// <see cref="ScaleWeightPolling.Continuous"/> is the default value.
        /// </summary>
        /// <param name="polling"></param>
        public abstract void StartPolling(ScaleWeightPolling polling = ScaleWeightPolling.Continuous);

        protected virtual void OnWeightReceived(ScaleEventArgs e) {
            WeightReceived?.Invoke(this, e);
        }

        public abstract void Dispose();
    }
}
