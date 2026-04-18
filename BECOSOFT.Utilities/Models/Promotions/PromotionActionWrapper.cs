using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Numeric;
using System;
using System.Globalization;

namespace BECOSOFT.Utilities.Models.Promotions {
    public class PromotionActionWrapper {
        public int PromotionID { get; set; }
        public string ConditionGroup { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string Value3 { get; set; }
        public string Value4 { get; set; }
        public string Value5 { get; set; }
        public string Value6 { get; set; }
        public string Value7 { get; set; }
        public string Amount { get; set; }

        public PromotionActionType Type { get; set; }

        /// <summary>
        /// Get the value assosiacted with the given <see cref="index"/>. If the value is <see langword="null"/> or an empty string, the <see langword="default"/> value of the provided <typeparamref name="T"/> type.
        /// </summary>
        /// <param name="index"></param>
        /// <typeparam name="T">Return type</typeparam>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if an invalid <paramref name="index"/> is given (<paramref name="index"/> &lt; 1 or <paramref name="index"/> &gt; 7).</exception>
        public T GetValue<T>(int index) {
            string value;
            switch (index) {
                case 1:
                    value = Value1;
                    break;
                case 2:
                    value = Value2;
                    break;
                case 3:
                    value = Value3;
                    break;
                case 4:
                    value = Value4;
                    break;
                case 5:
                    value = Value5;
                    break;
                case 6:
                    value = Value6;
                    break;
                case 7:
                    value = Value7;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            value = (value ?? "");
            var dotIndex = value.IndexOf('.');
            var commaIndex = value.IndexOf(',');
            if ((dotIndex >= 0 || commaIndex >= 0) && CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator
                                                   && CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator == CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator) {
                if (dotIndex < 0 && commaIndex >= 0) {
                    value = value.Replace(',', '.');
                } else if (dotIndex >= 0 && commaIndex < 0) {
                    value = value.Replace('.', ',');
                }
            }
            var res = value.IsNullOrEmpty() ? default(T) : value.To<T>();
            return res;
        }

        private string DebuggerDisplay => $"{Type}: {GetDescription()}";

        private string GetDescription() {
            switch (Type) {
                case PromotionActionType.Discount:
                    return $"D1: {Value1}, D2: {Value2}, D3: {Value3}, D4: {Value4}, D5: {Value5}, DiscountAmount: {Value6}, Base Price: {Value7}";
                case PromotionActionType.AddArticleWithPrice:
                    return $"ArticleID: {Value1.Truncate(50)}, Amount: {Amount}, Price: {Value3}";
                case PromotionActionType.ChangeNetPrice:
                    return $"ArticleID: {Value1.Truncate(50)}, Price: {Value3}";
                case PromotionActionType.ChangeNetPriceNotInPromo:
                    return "";
                case PromotionActionType.AddArticleWithDiscount:
                    return $"ArticleID: {Value1.Truncate(50)}, Amount: {Amount}, Discount: {Value3}";
                case PromotionActionType.ArticleTypePrice:
                    return $"Type: {Value2.To<PromotionActionArticleType>()}, Amount: {Value1}, Price: {Amount}, Discount: {Value3}, DiscountAmount: {Value4}, Secondary type: {Value5.To<PromotionActionArticleType?>()}";
                case PromotionActionType.Voucher:
                    return $"Price: {Amount}, Percentage: {Value1}";
                case PromotionActionType.PointFactor:
                    return $"Factor: {Value1}";
                case PromotionActionType.ProgressiveDiscount:
                    return $"Type: {Value1.To<PromotionActionArticleType>()}, D1: {Value1}, D2: {Value2}, D3: {Value3}";
                case PromotionActionType.ActivatePromotion:
                    return $"Activate Promotion {Value1}";
                default:
                    return "";
            }
        }

        public ArticleDataAdjustment ToAdjustment(decimal quantity, int promotionID) {
            var adjustedArticle = new ArticleDataAdjustment {
                Quantity = quantity,
                PromotionID = promotionID,
            };
            switch (Type) {
                case PromotionActionType.Discount:
                    for (var i = 1; i <= 5; i++) {
                        var value = GetValue<decimal?>(i)?.RoundTo(2);
                        adjustedArticle.SetDiscount(i, value);
                    }
                    adjustedArticle.DiscountAmount = GetValue<decimal?>(6)?.RoundTo(2);
                    adjustedArticle.BasePrice = GetValue<decimal?>(7)?.RoundTo(2);
                    break;
                case PromotionActionType.ChangeNetPrice:
                    adjustedArticle.BasePrice = GetValue<decimal?>(3)?.RoundTo(2);
                    break;
                case PromotionActionType.PointFactor:
                    adjustedArticle.PointFactor = GetValue<decimal?>(1);
                    break;
            }
            return adjustedArticle;
        }

        public PromotionActionWrapper Copy() {
            return new PromotionActionWrapper {
                PromotionID = PromotionID,
                ConditionGroup = ConditionGroup,
                Amount = Amount,
                Type = Type,
                Value1 = Value1,
                Value2 = Value2,
                Value3 = Value3,
                Value4 = Value4,
                Value5 = Value5,
                Value6 = Value6,
                Value7 = Value7,
            };
        }
    }
}