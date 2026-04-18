namespace BECOSOFT.Utilities.Models {
    /// <summary>
    /// The type of casing of a text
    /// </summary>
    public enum TextCasing {
        /// <summary>
        /// Normal, no casing
        /// </summary>
        Normal,
        /// <summary>
        /// All lowercase, depending on the current culture
        /// </summary>
        Lower,
        /// <summary>
        /// All uppercase, depending on the current culture
        /// </summary>
        Upper,
        /// <summary>
        /// All lowercase, independent of the current culture
        /// </summary>
        LowerInvariantCulture,
        /// <summary>
        /// All uppercase, independent of the current culture
        /// </summary>
        UpperInvariantCulture,
        /// <summary>
        /// All titlecase, depending on the current culture
        /// </summary>
        TitleCase,
        /// <summary>
        /// All titlecase, independent of the current culture
        /// </summary>
        TitleCaseInvariantCulture
    }
}
