using BECOSOFT.Utilities.Attributes;

namespace BECOSOFT.Utilities.Models {
    public enum MetricWeightUnit {
        /// <summary>
        /// Picogram (10^-12)
        /// </summary>
        [Abbreviation("pg")]
        [LocalizedEnum("MetricWeightUnit_Picogram", NameResourceType = typeof(Resources))]
        Picogram = -12,

        /// <summary>
        /// Nanogram (10^-9)
        /// </summary>
        [Abbreviation("ng")]
        [LocalizedEnum("MetricWeightUnit_Nanogram", NameResourceType = typeof(Resources))]
        Nanogram = -9,

        /// <summary>
        /// Microgram (10^-6)
        /// </summary>
        [Abbreviation("µg")]
        [LocalizedEnum("MetricWeightUnit_Microgram", NameResourceType = typeof(Resources))]
        Microgram = -6,

        /// <summary>
        /// Milligram (10^-3)
        /// </summary>
        [Abbreviation("mg")]
        [LocalizedEnum("MetricWeightUnit_Milligram", NameResourceType = typeof(Resources))]
        Milligram = -3,

        /// <summary>
        /// Base (10^0)
        /// </summary>
        [Abbreviation("g")]
        [LocalizedEnum("MetricWeightUnit_Gram", NameResourceType = typeof(Resources))]
        Gram = 0,

        /// <summary>
        /// Deca (10^1)
        /// </summary>
        [Abbreviation("dag")]
        [LocalizedEnum("MetricWeightUnit_Decagram", NameResourceType = typeof(Resources))]
        Decagram = 1,

        /// <summary>
        /// Hecto (10^2)
        /// </summary>
        [Abbreviation("hg")]
        [LocalizedEnum("MetricWeightUnit_Hectogram", NameResourceType = typeof(Resources))]
        Hectogram = 2,

        /// <summary>
        /// Kilo (10^3)
        /// </summary>
        [Abbreviation("kg")]
        [LocalizedEnum("MetricWeightUnit_Kilogram", NameResourceType = typeof(Resources))]
        Kilogram = 3,

        /// <summary>
        /// Tonne (10^6)
        /// </summary>
        [Abbreviation("t")]
        [LocalizedEnum("MetricWeightUnit_Tonne", NameResourceType = typeof(Resources))]
        Tonne = 6,
    }
}