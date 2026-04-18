using System;

namespace BECOSOFT.Utilities.Models {
    /// <summary>
    /// The precision of a <see cref="DateTime"/>
    /// </summary>
    public enum DateTimePrecision {
        /// <summary>
        /// Exactly to the hour
        /// </summary>
        Hour,
        /// <summary>
        /// Exactly to the minute
        /// </summary>
        Minute,
        /// <summary>
        /// Exactly to the second
        /// </summary>
        Second
    }
}