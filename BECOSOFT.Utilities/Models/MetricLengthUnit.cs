using BECOSOFT.Utilities.Attributes;

namespace BECOSOFT.Utilities.Models {
    public enum MetricLengthUnit {
        /// <summary>
        /// Picometre (10^-12)
        /// </summary>
        [Abbreviation("pm")]
        [LocalizedEnum("MetricLengthUnit_Picometre", NameResourceType = typeof(Resources))]
        Picometre = -12,

        /// <summary>
        /// Nanometre (10^-9)
        /// </summary>
        [Abbreviation("nm")]
        [LocalizedEnum("MetricLengthUnit_Nanometre", NameResourceType = typeof(Resources))]
        Nanometre = -9,

        /// <summary>
        /// Micrometre (10^-6)
        /// </summary>
        [Abbreviation("µm")]
        [LocalizedEnum("MetricLengthUnit_Micrometre", NameResourceType = typeof(Resources))]
        Micrometre = -6,

        /// <summary>
        /// Millimetre (10^-3)
        /// </summary>
        [Abbreviation("mm")]
        [LocalizedEnum("MetricLengthUnit_Millimetre", NameResourceType = typeof(Resources))]
        Millimetre = -3,

        /// <summary>
        /// Centimetre (10^-2)
        /// </summary>
        [Abbreviation("cm")]
        [LocalizedEnum("MetricLengthUnit_Centimetre", NameResourceType = typeof(Resources))]
        Centimetre = -2,

        /// <summary>
        /// Decimetre (10^-1)
        /// </summary>
        [Abbreviation("dm")]
        [LocalizedEnum("MetricLengthUnit_Decimetre", NameResourceType = typeof(Resources))]
        Decimetre = -1,

        /// <summary>
        /// Metre (10^0)
        /// </summary>
        [Abbreviation("m")]
        [LocalizedEnum("MetricLengthUnit_Metre", NameResourceType = typeof(Resources))]
        Metre = 0,

        /// <summary>
        /// Decametre (10^1)
        /// </summary>
        [Abbreviation("dam")]
        [LocalizedEnum("MetricLengthUnit_Decametre", NameResourceType = typeof(Resources))]
        Decametre = 1,

        /// <summary>
        /// Hectometre (10^2)
        /// </summary>
        [Abbreviation("hm")]
        [LocalizedEnum("MetricLengthUnit_Hectometre", NameResourceType = typeof(Resources))]
        Hectometre = 2,

        /// <summary>
        /// Kilometre (10^3)
        /// </summary>
        [Abbreviation("km")]
        [LocalizedEnum("MetricLengthUnit_Kilometre", NameResourceType = typeof(Resources))]
        Kilometre = 3,
    }
}