using BECOSOFT.Utilities.Attributes;

namespace BECOSOFT.Utilities.Models {
    public enum MetricAreaUnit {
        /// <summary>
        /// Square millimetre (10^-6)
        /// </summary>
        [Abbreviation("mm²")]
        [LocalizedEnum("MetricLengthUnit_SquareMillimetre", NameResourceType = typeof(Resources))]
        Millimetre = -6,

        /// <summary>
        /// Square centimetre (10^-2)
        /// </summary>
        [Abbreviation("cm²")]
        [LocalizedEnum("MetricLengthUnit_SquareCentimetre", NameResourceType = typeof(Resources))]
        Centimetre = -4,

        /// <summary>
        /// Square decimetre (10^-1)
        /// </summary>
        [Abbreviation("dm²")]
        [LocalizedEnum("MetricLengthUnit_SquareDecimetre", NameResourceType = typeof(Resources))]
        Decimetre = -2,

        /// <summary>
        /// Square metre (Centiare) (10^0)
        /// </summary>
        [Abbreviation("m²")]
        [LocalizedEnum("MetricLengthUnit_SquareMetre", NameResourceType = typeof(Resources))]
        Metre = 0,

        /// <summary>
        /// Square decametre (Decare) (10^2)
        /// </summary>
        [Abbreviation("dam²")]
        [LocalizedEnum("MetricLengthUnit_SquareDecametre", NameResourceType = typeof(Resources))]
        Decametre = 2,

        /// <summary>
        /// Square hectometre (Hectare) (10^4)
        /// </summary>
        [Abbreviation("hm²")]
        [LocalizedEnum("MetricLengthUnit_SquareHectometre", NameResourceType = typeof(Resources))]
        Hectometre = 4,

        /// <summary>
        /// Square kilometre (10^6)
        /// </summary>
        [Abbreviation("km²")]
        [LocalizedEnum("MetricLengthUnit_SquareKilometre", NameResourceType = typeof(Resources))]
        Kilometre = 6,
    }
}