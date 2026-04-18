using BECOSOFT.Utilities.Attributes;

namespace BECOSOFT.Utilities.Models.Promotions {
    public enum PromotionConditionType {
        [LocalizedEnum(nameof(Resources.PromotionConditionType_None), NameResourceType = typeof(Resources))]
        None,
        [LocalizedEnum(nameof(Resources.PromotionConditionType_ArticleID), NameResourceType = typeof(Resources))]
        ArticleID,
        [LocalizedEnum(nameof(Resources.PromotionConditionType_Brand), NameResourceType = typeof(Resources))]
        Brand,
        [LocalizedEnum(nameof(Resources.PromotionConditionType_Group), NameResourceType = typeof(Resources))]
        Group,
        [LocalizedEnum(nameof(Resources.PromotionConditionType_DiscountGroup), NameResourceType = typeof(Resources))]
        DiscountGroup,
        [LocalizedEnum(nameof(Resources.PromotionConditionType_DocumentValue), NameResourceType = typeof(Resources))]
        DocumentValue,
        [LocalizedEnum(nameof(Resources.PromotionConditionType_AllArticles), NameResourceType = typeof(Resources))]
        AllArticles,
        [LocalizedEnum(nameof(Resources.PromotionConditionType_ProductValue), NameResourceType = typeof(Resources))]
        ProductValue,
        [LocalizedEnum(nameof(Resources.PromotionConditionType_Tag), NameResourceType = typeof(Resources))]
        Tag,
        [LocalizedEnum(nameof(Resources.PromotionConditionType_BrandValue), NameResourceType = typeof(Resources))]
        BrandValue,
        [LocalizedEnum(nameof(Resources.PromotionConditionType_FashionColor), NameResourceType = typeof(Resources))]
        FashionColor,
        [LocalizedEnum(nameof(Resources.PromotionConditionType_FashionCollection), NameResourceType = typeof(Resources))]
        FashionCollection,
        [LocalizedEnum(nameof(Resources.PromotionConditionType_FashionFabric), NameResourceType = typeof(Resources))]
        FashionFabric,
        [LocalizedEnum(nameof(Resources.PromotionConditionType_Weight), NameResourceType = typeof(Resources))]
        Weight,
        [LocalizedEnum(nameof(Resources.PromotionConditionType_DocumentValueExcludingOtherPromotion), NameResourceType = typeof(Resources))]
        DocumentValueExcludingOtherPromotion,
        [LocalizedEnum(nameof(Resources.PromotionConditionType_Season), NameResourceType = typeof(Resources))]
        Season,
        [LocalizedEnum(nameof(Resources.PromotionConditionType_Matrix), NameResourceType = typeof(Resources))]
        Matrix,
        [LocalizedEnum(nameof(Resources.PromotionConditionType_PromotionArticleFilter), NameResourceType = typeof(Resources))]
        PromotionArticleFilter,
        [LocalizedEnum(nameof(Resources.PromotionConditionType_Type), NameResourceType = typeof(Resources))]
        Type,
    }
}