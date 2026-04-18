using BECOSOFT.Utilities.Attributes;

namespace BECOSOFT.Utilities.Models.Global {
    public enum OffsetType : byte {
        [LocalizedEnum(nameof(Resources.OffsetType_None), NameResourceType = typeof(Resources))]
        None = 0,

        [LocalizedEnum(nameof(Resources.OffsetType_Year), NameResourceType = typeof(Resources))]
        Year = 1,

        [LocalizedEnum(nameof(Resources.OffsetType_Month), NameResourceType = typeof(Resources))]
        Month = 2,

        [LocalizedEnum(nameof(Resources.OffsetType_Day), NameResourceType = typeof(Resources))]
        Day = 3,
    }
}