using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Models.Promotions;

namespace BECOSOFT.Utilities.Promotions {
    public static class PromotionConverter {
        private const string Per = "Per";
        private const string From = "Vanaf";
        private const string ArticleID = "Artikelnummer";
        private const string Brand = "Merk";
        private const string Group = "Groep";
        private const string DiscountGroup = "Kortingsgroep";
        private const string DocumentValue = "Ticketwaarde";
        private const string AllArticles = "Alle artikelen";
        private const string ProductValue = "Productwaarde";
        private const string BrandValue = "Merkwaarde";
        private const string FashionColor = nameof(FashionColor);
        private const string FashionCollection = nameof(FashionCollection);
        private const string FashionFabric = nameof(FashionFabric);
        private const string Weight = nameof(Weight);
        private const string PromotionArticleFilter = nameof(PromotionArticleFilter);
        private const string Type = nameof(Type);
        private const string Tag = "Tags";
        private const string DocumentValueExcludingOtherPromotion = nameof(DocumentValueExcludingOtherPromotion);
        private const string Season = nameof(Season);
        private const string Matrix = nameof(Matrix);
        private const string EqualTo = "Gelijk is aan";
        private const string GreaterOrEqual = "Groter of gelijk is aan";
        private const string SmallerThan = "Kleiner is dan";
        private const string DifferentFrom = "Verschillend is van";
        private const string TypeOperatorDifferentFromValue = "not";
        private const string ActionDiscount = "GEEFKORTING";
        private const string ActionAddArticle = "VOEGARTIKELSTOE";
        private const string ActionChangeNetPrice = "WIJZIGNAARNETTOPRIJS";
        private const string ActionChangeNetPriceNotInPromo = "WIJZIGNAARNETTOPRIJSNIETINPROMO";
        private const string ActionAddArticleWithDiscount = "VOEGARTTOEMETKORTING";
        private const string ActionArticleTypePrice = "TYPEPRIJS";
        private const string ActionValueVoucher = "VALUEVOUCHER";
        private const string ActionPointFactor = "FACTOR";
        private const string ActionProgressiveDiscount = "PROGRESSIEVEKORTING";
        private const string ActionActivatePromotion = "ACTIVATEPROMOTION";

        public static PromotionGrouping ConvertGroupingValue(string groupingValue) {
            if (groupingValue.EqualsIgnoreCase("per")) {
                return PromotionGrouping.Per;
            }
            if (groupingValue.EqualsIgnoreCase("v-per")) {
                return PromotionGrouping.FromPer;
            }
            return PromotionGrouping.From;
        }

        public static string ConvertGrouping(PromotionGrouping grouping) {
            switch (grouping) {
                case PromotionGrouping.From:
                    return From;
                case PromotionGrouping.Per:
                default:
                    return Per;
            }
        }

        public static PromotionConditionType ConvertConditionTypeValue(string conditionTypeValue) {
            switch (conditionTypeValue) {
                case ArticleID:
                    return PromotionConditionType.ArticleID;
                case Brand:
                    return PromotionConditionType.Brand;
                case Group:
                    return PromotionConditionType.Group;
                case DiscountGroup:
                    return PromotionConditionType.DiscountGroup;
                case DocumentValue:
                    return PromotionConditionType.DocumentValue;
                case AllArticles:
                    return PromotionConditionType.AllArticles;
                case ProductValue:
                    return PromotionConditionType.ProductValue;
                case Tag:
                    return PromotionConditionType.Tag;
                case BrandValue:
                    return PromotionConditionType.BrandValue;
                case FashionColor:
                    return PromotionConditionType.FashionColor;
                case FashionCollection:
                    return PromotionConditionType.FashionCollection;
                case FashionFabric:
                    return PromotionConditionType.FashionFabric;
                case Weight:
                    return PromotionConditionType.Weight;
                case DocumentValueExcludingOtherPromotion:
                    return PromotionConditionType.DocumentValueExcludingOtherPromotion;
                case Season:
                    return PromotionConditionType.Season;
                case Matrix:
                    return PromotionConditionType.Matrix;
                case PromotionArticleFilter:
                    return PromotionConditionType.PromotionArticleFilter;
                case Type:
                    return PromotionConditionType.Type;
                default:
                    return PromotionConditionType.ArticleID;
            }
        }

        public static string ConvertType(PromotionConditionType conditionType) {
            switch (conditionType) {
                case PromotionConditionType.ArticleID:
                    return ArticleID;
                case PromotionConditionType.Brand:
                    return Brand;
                case PromotionConditionType.Group:
                    return Group;
                case PromotionConditionType.DiscountGroup:
                    return DiscountGroup;
                case PromotionConditionType.DocumentValue:
                    return DocumentValue;
                case PromotionConditionType.AllArticles:
                    return AllArticles;
                case PromotionConditionType.ProductValue:
                    return ProductValue;
                case PromotionConditionType.Tag:
                    return Tag;
                case PromotionConditionType.BrandValue:
                    return BrandValue;
                case PromotionConditionType.FashionColor:
                    return FashionColor;
                case PromotionConditionType.FashionCollection:
                    return FashionCollection;
                case PromotionConditionType.FashionFabric:
                    return FashionFabric;
                case PromotionConditionType.Weight:
                    return Weight;
                case PromotionConditionType.DocumentValueExcludingOtherPromotion:
                    return DocumentValueExcludingOtherPromotion;
                case PromotionConditionType.Season:
                    return Season;
                case PromotionConditionType.Matrix:
                    return Matrix;
                case PromotionConditionType.PromotionArticleFilter:
                    return PromotionArticleFilter;
                case PromotionConditionType.Type:
                    return Type;
                default:
                    return ArticleID;
            }
        }

        public static PromotionQuantityComparisonOperator ConvertQuantityComparisonOperatorValue(string operatorValue) {
            switch (operatorValue) {
                case GreaterOrEqual:
                    return PromotionQuantityComparisonOperator.GreaterOrEqual;
                case SmallerThan:
                    return PromotionQuantityComparisonOperator.SmallerThan;
                case DifferentFrom:
                    return PromotionQuantityComparisonOperator.DifferentFrom;
                default:
                    return PromotionQuantityComparisonOperator.GreaterOrEqual;
            }
        }

        public static string ConvertQuantityComparisonOperator(PromotionQuantityComparisonOperator promotionQuantityComparisonOperator) {
            switch (promotionQuantityComparisonOperator) {
                case PromotionQuantityComparisonOperator.GreaterOrEqual:
                    return GreaterOrEqual;
                case PromotionQuantityComparisonOperator.SmallerThan:
                    return SmallerThan;
                case PromotionQuantityComparisonOperator.DifferentFrom:
                    return DifferentFrom;
                default:
                    return GreaterOrEqual;
            }
        }

        public static PromotionTypeOperator ConvertTypeOperatorValue(string operatorValue) {
            switch (operatorValue) {
                case TypeOperatorDifferentFromValue:
                    return PromotionTypeOperator.DifferentFrom;
                default:
                    return PromotionTypeOperator.EqualTo;
            }
        }

        public static string ConvertTypeOperator(PromotionTypeOperator promotionOperator) {
            switch (promotionOperator) {
                case PromotionTypeOperator.DifferentFrom:
                    return TypeOperatorDifferentFromValue;
                case PromotionTypeOperator.EqualTo:
                default:
                    return string.Empty;
            }
        }

        public static PromotionActionType ConvertActionTypeValue(string actionTypeValue) {
            switch (actionTypeValue) {
                case ActionAddArticle:
                    return PromotionActionType.AddArticleWithPrice;
                case ActionChangeNetPrice:
                    return PromotionActionType.ChangeNetPrice;
                case ActionChangeNetPriceNotInPromo:
                    return PromotionActionType.ChangeNetPriceNotInPromo;
                case ActionAddArticleWithDiscount:
                    return PromotionActionType.AddArticleWithDiscount;
                case ActionArticleTypePrice:
                    return PromotionActionType.ArticleTypePrice;
                case ActionValueVoucher:
                    return PromotionActionType.Voucher;
                case ActionPointFactor:
                    return PromotionActionType.PointFactor;
                case ActionProgressiveDiscount:
                    return PromotionActionType.ProgressiveDiscount;
                case ActionActivatePromotion:
                    return PromotionActionType.ActivatePromotion;
                default:
                    return PromotionActionType.Discount;
            }
        }

        public static string ConvertActionType(PromotionActionType actionType) {
            switch (actionType) {
                case PromotionActionType.AddArticleWithPrice:
                    return ActionAddArticle;
                case PromotionActionType.ChangeNetPrice:
                    return ActionChangeNetPrice;
                case PromotionActionType.ChangeNetPriceNotInPromo:
                    return ActionChangeNetPriceNotInPromo;
                case PromotionActionType.AddArticleWithDiscount:
                    return ActionAddArticleWithDiscount;
                case PromotionActionType.ArticleTypePrice:
                    return ActionArticleTypePrice;
                case PromotionActionType.Voucher:
                    return ActionValueVoucher;
                case PromotionActionType.PointFactor:
                    return ActionPointFactor;
                case PromotionActionType.ProgressiveDiscount:
                    return ActionProgressiveDiscount;
                case PromotionActionType.ActivatePromotion:
                    return ActionActivatePromotion;
                default:
                    return ActionDiscount;
            }
        }
    }
}