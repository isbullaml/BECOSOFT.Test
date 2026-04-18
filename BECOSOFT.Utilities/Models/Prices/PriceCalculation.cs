using BECOSOFT.Utilities.Extensions.Numeric;
using System;

namespace BECOSOFT.Utilities.Models.Prices {
    public class PriceCalculation : DiscountHolder {
        public int ArticleID { get; set; }
        public int PriceTypeID { get; set; }
        public decimal Quantity { get; set; } = 1;

        /// <summary>
        /// <para>Base price including the cost-code price and multiplier.</para>
        /// <para>(Price + CostCodePrice) * CostCodeMultiplier</para>
        /// </summary>
        public decimal BasePrice { get; set; }

        /// <summary>
        /// <para>Base price (excl. the promo-prices) including the cost-code price and multiplier.</para>
        /// <para>(Price + CostCodePrice) * CostCodeMultiplier</para>
        /// </summary>
        public decimal BasePriceWithoutPromotion { get; set; }

        /// <summary>
        /// <para>Combined percentage multiplier.</para>
        /// <para>ABS(1 - (Percentage / 100))</para>
        /// </summary>
        public decimal PercentageMultiplier { get; set; } = 1m;

        /// <summary>
        /// <para>Combined percentage multiplier (excl. the promo-prices).</para>
        /// <para>ABS(1 - (Percentage / 100))</para>
        /// </summary>
        public decimal PercentageMultiplierWithoutPromotion { get; set; } = 1m;

        /// <summary>
        /// <para>Discount1 from the contact-price if a contact-price exists, otherwise Discount1 from the article-price (incl. promo-prices).</para>
        /// </summary>
        public override decimal Discount1 { get; set; }

        /// <summary>
        /// <para>Discount2 from the contact-price if a contact-price exists, otherwise Discount2 from the article-price (incl. promo-prices).</para>
        /// </summary>
        public override decimal Discount2 { get; set; }

        /// <summary>
        /// <para>Discount3 from the contact-price if a contact-price exists, otherwise Discount3 from the article-price (incl. promo-prices).</para>
        /// </summary>
        public override decimal Discount3 { get; set; }

        /// <summary>
        /// <para>Discount4 from the contact-price if a contact-price exists, otherwise Discount4 from the article-price (incl. promo-prices).</para>
        /// </summary>
        public override decimal Discount4 { get; set; }

        /// <summary>
        /// <para>Discount5 from the contact-price if a contact-price exists, otherwise Discount5 from the article-price (incl. promo-prices).</para>
        /// </summary>
        public override decimal Discount5 { get; set; }

        /// <summary>
        /// <para>Amount of rounding digits to be used. Default = 2.</para>
        /// </summary>
        public int RoundingDigits { get; set; } = 2;

        /// <summary>
        /// <para>Contact-price if a contact-price exists. Otherwise this is <see langword="null"/>.</para>
        /// </summary>
        public decimal? ContactPrice { get; set; }

        /// <summary>
        /// <para>Combined contact-percentage multiplier if a contact-price exists. Otherwise this is 1.</para>
        /// <para>ABS(1 - (Percentage / 100))</para>
        /// </summary>
        public decimal ContactPercentageMultiplier { get; set; } = 1m;

        /// <summary>
        /// <para>VATgroup-ID of the contact-price if it exists. Otherwise the VATgroup-ID of the article.</para>
        /// </summary>
        public int VatGroupID { get; set; }

        /// <summary>
        /// <para>VAT percentage of the contact-price if it exists. Otherwise the VAT percentage of the article.</para>
        /// </summary>
        public decimal VatPercentage { get; set; }

        /// <summary>
        /// <para>Indicates whether the results are VAT-inclusive.</para>
        /// </summary>
        public bool IsInclusive { get; set; }

        /// <summary>
        /// Indicates the starting point for this <see cref="PriceCalculation"/>.
        /// </summary>
        public DateTime From { get; set; }

        /// <summary>
        /// Indicates the end point for this <see cref="PriceCalculation"/>.
        /// </summary>
        public DateTime To { get; set; }

        /// <summary>
        /// Range based on the current <see cref="From"/> and <see cref="To"/> values.
        /// </summary>
        public Range<DateTime> DateRange => new Range<DateTime>(From, To);

        /// <summary>
        /// Indicates that the price data is from a margin price line
        /// </summary>
        public bool MarginPrice { get; set; }

        /// <summary>
        /// Indicates that the price data is from a promotion margin price line
        /// </summary>
        public bool PromoMarginPrice { get; set; }

        /// <summary>
        /// Indicates that the <see cref="Discount1"/> to <see cref="Discount5"/> are from contact discount groups.
        /// </summary>
        public bool DiscountFromContact { get; set; }

        public bool HasPromotionPrice => BasePriceWithoutPromotion != BasePrice;
        public bool HasContactPrice => ContactPrice.HasValue || ContactPercentageMultiplier != 1;

        /// <summary>
        /// <para>The calculated price (incl. promo-prices).</para>
        /// <para>1. The <see cref="ContactPrice"/> if it's not <see langword="null"/>.</para>
        /// <para>2. The <see cref="BasePrice"/> multiplied with the <see cref="ContactPercentageMultiplier"/> if the multiplier is not 1.</para>
        /// <para>3. Returns 0 otherwise.</para>
        /// </summary>
        public decimal CalculatedContactPrice {
            get {
                if (ContactPrice.HasValue) {
                    return ContactPrice.Value;
                }

                if (ContactPercentageMultiplier != 1m) {
                    return BasePrice * ContactPercentageMultiplier;
                }

                return 0m;
            }
        }

        /// <summary>
        /// <para>The calculated price (incl. promo-prices).</para>
        /// <para>1. The <see cref="ContactPrice"/> if it's not <see langword="null"/>.</para>
        /// <para>2. The <see cref="BasePrice"/> multiplied with the <see cref="ContactPercentageMultiplier"/> if the multiplier is not 1.</para>
        /// <para>3. The <see cref="BasePrice"/> multiplied with the <see cref="PercentageMultiplier"/> otherwise.</para>
        /// </summary>
        public decimal CalculatedPrice {
            get {
                var calculatedContactPrice = CalculatedContactPrice;
                if (calculatedContactPrice != 0m) {
                    return calculatedContactPrice;
                }

                return BasePrice * PercentageMultiplier;
            }
        }

        /// <summary>
        /// <para>The calculated price (excl. promo-prices).</para>
        /// <para>1. The <see cref="ContactPrice"/> if it's not <see langword="null"/>.</para>
        /// <para>2. The <see cref="BasePriceWithoutPromotion"/> multiplied with the <see cref="ContactPercentageMultiplier"/> if the multiplier is not 1.</para>
        /// <para>3. The <see cref="BasePriceWithoutPromotion"/> multiplied with the <see cref="PercentageMultiplierWithoutPromotion"/> otherwise.</para>
        /// </summary>
        public decimal CalculatedPriceWithoutPromotion {
            get {
                if (ContactPrice.HasValue) {
                    return ContactPrice.Value;
                }

                if (ContactPercentageMultiplier != 1m) {
                    return BasePriceWithoutPromotion * ContactPercentageMultiplier;
                }

                return BasePriceWithoutPromotion * PercentageMultiplierWithoutPromotion;
            }
        }

        /// <summary>
        /// <see cref="CalculatedPrice"/>, rounded using <see cref="RoundingDigits"/>.
        /// </summary>
        public decimal RoundedCalculatedPrice => CalculatedPrice.RoundTo(RoundingDigits);

        /// <summary>
        /// The <see cref="CalculatedPrice"/> with al the discounts (<see cref="DiscountHolder.DiscountMultiplier"/>).
        /// </summary>
        public decimal PriceWithDiscount => CalculatedPrice * DiscountMultiplier;

        /// <summary>
        /// <see cref="PriceWithDiscount"/>, rounded using <see cref="RoundingDigits"/>.
        /// </summary>
        public decimal RoundedPriceWithDiscount => PriceWithDiscount.RoundTo(RoundingDigits);

        /// <summary>
        /// The <see cref="CalculatedPriceWithoutPromotion"/> with al the discounts (<see cref="DiscountHolder.DiscountMultiplier"/>).
        /// </summary>
        public decimal PriceWithoutPromotionWithDiscount => CalculatedPriceWithoutPromotion * DiscountMultiplier;

        /// <summary>
        /// <see cref="PriceWithoutPromotionWithDiscount"/>, rounded using <see cref="RoundingDigits"/>.
        /// </summary>
        public decimal RoundedPriceWithoutPromotionWithDiscount => PriceWithoutPromotionWithDiscount.RoundTo(RoundingDigits);

        /// <summary>
        /// <see cref="BasePriceWithoutPromotion"/>, rounded using <see cref="RoundingDigits"/>.
        /// </summary>
        public decimal RoundedBasePriceWithoutPromotion => BasePriceWithoutPromotion.RoundTo(RoundingDigits);

        /// <summary>
        /// <see cref="PriceCalculation"/> is empty and has no actual price data
        /// </summary>
        public bool IsEmpty { get; private set; }

        public static PriceCalculation Empty =>
            new PriceCalculation {
                IsEmpty = true,
            };
    }
}