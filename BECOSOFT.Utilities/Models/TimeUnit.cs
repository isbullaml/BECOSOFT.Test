using BECOSOFT.Utilities.Attributes;

namespace BECOSOFT.Utilities.Models {
    public enum TimeUnit : byte {
        [LocalizedEnum("TimeUnit_Undefined", NameResourceType = typeof(Resources))]
        [LocalizedPluralEnum("TimeUnit_Undefined", NameResourceType = typeof(Resources))]
        Undefined = 0,
        [LocalizedEnum("TimeUnit_Year", NameResourceType = typeof(Resources))]
        [LocalizedPluralEnum("TimeUnit_Year_Plural", NameResourceType = typeof(Resources))]
        Year = 1,
        [LocalizedEnum("TimeUnit_Month", NameResourceType = typeof(Resources))]
        [LocalizedPluralEnum("TimeUnit_Month_Plural", NameResourceType = typeof(Resources))]
        Month = 2,
        [LocalizedEnum("TimeUnit_Week", NameResourceType = typeof(Resources))]
        [LocalizedPluralEnum("TimeUnit_Week_Plural", NameResourceType = typeof(Resources))]
        Week = 3,
        [LocalizedEnum("TimeUnit_Day_Plural", NameResourceType = typeof(Resources))]
        [LocalizedPluralEnum("TimeUnit_Day_Plural", NameResourceType = typeof(Resources))]
        Day = 4
    }
}
