using BECOSOFT.Utilities.Attributes;

namespace BECOSOFT.Utilities.Models {
    public enum MetricCubicVolumeUnit {
        /// <summary>
        /// Cubic millimetre (10^-9)
        /// </summary>
        [Abbreviation("mm³")]
        [LocalizedEnum("MetricLengthUnit_CubicMillimetre", NameResourceType = typeof(Resources))]
        Millimetre = -9,

        /// <summary>
        /// Cubic centimetre (10^-6)
        /// </summary>
        [Abbreviation("cm³")]
        [LocalizedEnum("MetricLengthUnit_CubicCentimetre", NameResourceType = typeof(Resources))]
        Centimetre = -6,

        /// <summary>
        /// Cubic decimetre (10^-3)
        /// </summary>
        [Abbreviation("dm³")]
        [LocalizedEnum("MetricLengthUnit_CubicDecimetre", NameResourceType = typeof(Resources))]
        Decimetre = -3,

        /// <summary>
        /// Cubic metre (10^0)
        /// </summary>
        [Abbreviation("m³")]
        [LocalizedEnum("MetricLengthUnit_CubicMetre", NameResourceType = typeof(Resources))]
        Metre = 0,

        /// <summary>
        /// Cubic decametre (10^3)
        /// </summary>
        [Abbreviation("dam³")]
        [LocalizedEnum("MetricLengthUnit_CubicDecametre", NameResourceType = typeof(Resources))]
        Decametre = 3,

        /// <summary>
        /// Cubic hectometre (10^6)
        /// </summary>
        [Abbreviation("hm³")]
        [LocalizedEnum("MetricLengthUnit_CubicHectometre", NameResourceType = typeof(Resources))]
        Hectometre = 6,

        /// <summary>
        /// Cubic kilometre (10^9)
        /// </summary>
        [Abbreviation("km³")]
        [LocalizedEnum("MetricLengthUnit_CubicKilometre", NameResourceType = typeof(Resources))]
        Kilometre = 9,
    }
}