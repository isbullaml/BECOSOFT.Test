namespace BECOSOFT.Utilities.IO.Scales {
    public enum ScaleWeightPolling {
        /// <summary>
        /// Continuous: keep asking the scale for the weight
        /// </summary>
        Continuous = 0,

        /// Once: Ask the scale for the weight once
        Once = 1,
    }
}