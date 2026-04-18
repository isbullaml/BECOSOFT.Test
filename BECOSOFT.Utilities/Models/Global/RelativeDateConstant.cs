using BECOSOFT.Utilities.Attributes;

namespace BECOSOFT.Utilities.Models.Global {
    public enum RelativeDateConstant : byte {
        [LocalizedEnum(nameof(Resources.RelativeDateConstant_Unknown), NameResourceType = typeof(Resources))]
        Unknown = 0,
        [LocalizedEnum(nameof(Resources.RelativeDateConstant_Today), NameResourceType = typeof(Resources))]
        Today = 1,
        [LocalizedEnum(nameof(Resources.RelativeDateConstant_Now), NameResourceType = typeof(Resources))]
        Now = 2,
        [LocalizedEnum(nameof(Resources.RelativeDateConstant_StartOfYear), NameResourceType = typeof(Resources))]
        StartOfYear = 3,
        [LocalizedEnum(nameof(Resources.RelativeDateConstant_StartOfMonth), NameResourceType = typeof(Resources))]
        StartOfMonth = 4,
        [LocalizedEnum(nameof(Resources.RelativeDateConstant_EndOfMonth), NameResourceType = typeof(Resources))]
        EndOfMonth = 5,
        [LocalizedEnum(nameof(Resources.RelativeDateConstant_EndOfYear), NameResourceType = typeof(Resources))]
        EndOfYear = 6,
    }
}