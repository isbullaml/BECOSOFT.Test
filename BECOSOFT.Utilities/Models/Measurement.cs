using BECOSOFT.Utilities.Attributes;

namespace BECOSOFT.Utilities.Models {
    public enum Measurement {
        [Abbreviation("l")]
        [LocalizedAbbreviation("Measurement_Length_Abbreviation", NameResourceType = typeof(Resources))]
        [LocalizedEnum("Measurement_Length", NameResourceType = typeof(Resources))]
        Length,
        [Abbreviation("w")]
        [LocalizedAbbreviation("Measurement_Width_Abbreviation", NameResourceType = typeof(Resources))]
        [LocalizedEnum("Measurement_Width", NameResourceType = typeof(Resources))]
        Width,
        [Abbreviation("h")]
        [LocalizedAbbreviation("Measurement_Height_Abbreviation", NameResourceType = typeof(Resources))]
        [LocalizedEnum("Measurement_Height", NameResourceType = typeof(Resources))]
        Height,
        [LocalizedEnum("Measurement_Weight", NameResourceType = typeof(Resources))]
        Weight,
        [LocalizedEnum("Measurement_Gross", NameResourceType = typeof(Resources))]
        Gross,
        [LocalizedEnum("Measurement_Net", NameResourceType = typeof(Resources))]
        Net,
        [LocalizedEnum("Measurement_Tare", NameResourceType = typeof(Resources))]
        Tare,
    }
}