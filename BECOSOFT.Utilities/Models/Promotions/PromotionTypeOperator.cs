using BECOSOFT.Utilities.Attributes;

namespace BECOSOFT.Utilities.Models.Promotions {
    public enum PromotionTypeOperator {
        [LocalizedEnum(nameof(Resources.PromotionTypeOperator_EqualTo), NameResourceType = typeof(Resources))]
        EqualTo,
        [LocalizedEnum(nameof(Resources.PromotionTypeOperator_DifferentFrom), NameResourceType = typeof(Resources))]
        DifferentFrom
    }
}