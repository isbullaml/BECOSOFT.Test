using BECOSOFT.Utilities.Attributes;

namespace BECOSOFT.Utilities.Models {
    public enum Month {
        [LocalizedEnum("Month_January", NameResourceType = typeof(Resources))]
        January = 1,

        [LocalizedEnum("Month_February", NameResourceType = typeof(Resources))]
        February = 2,

        [LocalizedEnum("Month_March", NameResourceType = typeof(Resources))]
        March = 3,

        [LocalizedEnum("Month_April", NameResourceType = typeof(Resources))]
        April = 4,

        [LocalizedEnum("Month_May", NameResourceType = typeof(Resources))]
        May = 5,

        [LocalizedEnum("Month_June", NameResourceType = typeof(Resources))]
        June = 6,

        [LocalizedEnum("Month_July", NameResourceType = typeof(Resources))]
        July = 7,

        [LocalizedEnum("Month_August", NameResourceType = typeof(Resources))]
        August = 8,

        [LocalizedEnum("Month_September", NameResourceType = typeof(Resources))]
        September = 9,

        [LocalizedEnum("Month_October", NameResourceType = typeof(Resources))]
        October = 10,

        [LocalizedEnum("Month_November", NameResourceType = typeof(Resources))]
        November = 11,

        [LocalizedEnum("Month_December", NameResourceType = typeof(Resources))]
        December = 12
    }
}