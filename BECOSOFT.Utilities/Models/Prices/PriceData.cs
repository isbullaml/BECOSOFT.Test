using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Extensions.Numeric;
using BECOSOFT.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Models.Prices {
    public class PriceData {
        public int ArticleID { get; set; }
        public int PriceTypeID { get; set; }
        public decimal CostCodePrice { get; set; }
        public decimal CostCodePercentageMultiplier { get; set; } = 1m;
        public DiscountHolder ContactDiscounts { get; set; }
        public decimal ContactPercentage { get; set; }
        public bool IsInclusive { get; set; }

        public List<TierPriceData> TierPrices { get; set; } = new List<TierPriceData>();
        public List<ContactTierPriceData> ContactTierPrices { get; set; } = new List<ContactTierPriceData>();

        public PriceData() {
            ContactDiscounts = new DiscountHolder();
        }

        public decimal GetContactTierPricePercentageMultiplier(decimal quantity, bool withoutPromo = false) {
            var filteredTierPrices = ContactTierPrices.Where(tp => tp.PercentageMultiplier != 1m && tp.Quantity <= quantity);
            if (withoutPromo) {
                filteredTierPrices = filteredTierPrices.Where(tp => !tp.IsPromo);
            }

            return filteredTierPrices.Aggregate(1m, (value, tierPrice) => value * tierPrice.PercentageMultiplier);
        }

        public decimal GetTierPricePercentageMultiplier(decimal quantity, int supplierID = 0, bool withoutPromo = false) {
            var filteredTierPrices = TierPrices.Where(tp => tp.PercentageMultiplier != 1m && tp.Quantity <= quantity);
            if (supplierID != 0) {
                filteredTierPrices = filteredTierPrices.Where(tp => tp.SupplierID == supplierID);
            }

            if (withoutPromo) {
                filteredTierPrices = filteredTierPrices.Where(tp => !tp.IsPromo);
            }

            return filteredTierPrices.Aggregate(1m, (value, tierPrice) => value * tierPrice.PercentageMultiplier);
        }

        /// <summary>
        /// Gets the price-calculation parameters for a given <paramref name="quantity"/>
        /// </summary>
        /// <param name="quantity">The quantity to get the price-calculation for</param>
        /// <returns></returns>
        public PriceCalculation GetPriceCalculation(decimal quantity) {
            var orderedTierPrices = TierPrices.OrderByDescending(t => t.Quantity).ToList();
            var tierPrice = orderedTierPrices.FirstOrDefault(tp => tp.PercentageMultiplier == 1m && !tp.IsPromo && tp.Quantity <= quantity) ?? TierPriceData.Empty(ArticleID);
            var promotionTierPrice = orderedTierPrices.FirstOrDefault(tp => tp.PercentageMultiplier == 1m && tp.Quantity <= quantity && tp.IsPromo) ?? tierPrice;

            var orderedContactPrices = ContactTierPrices.OrderByDescending(t => t.Quantity).ToList();
            var contactTierPrice = orderedContactPrices.FirstOrDefault(tp => tp.PercentageMultiplier == 1m && tp.Quantity <= quantity) ?? ContactTierPriceData.Empty(ArticleID);

            var promotionPercentageMultipler = GetTierPricePercentageMultiplier(quantity, promotionTierPrice.SupplierID, false);
            var percentageMultipler = GetTierPricePercentageMultiplier(quantity, tierPrice.SupplierID, true);
            var contactPercentageMultiplier = GetContactTierPricePercentageMultiplier(quantity, false);

            var from = new[] { tierPrice.From, promotionTierPrice.From, contactTierPrice.From }.Where(IsValidPriceDate).Distinct().MaxOrDefault(DateTimeHelpers.SqlSmallDateTimeMinValue);
            var to = new[] { tierPrice.To, promotionTierPrice.To, contactTierPrice.To }.Where(IsValidPriceDate).Distinct().MinOrDefault(DateTimeHelpers.SqlDateTimeMaxValue);

            PriceCalculation calculation;
            if (contactTierPrice.IsEmpty && tierPrice.IsEmpty && promotionTierPrice.IsEmpty) {
                calculation = PriceCalculation.Empty;
            } else {
                calculation = new PriceCalculation();
            }
            calculation.ArticleID = ArticleID;
            calculation.PriceTypeID = PriceTypeID;
            calculation.Quantity = tierPrice.Quantity;
            calculation.BasePriceWithoutPromotion = (tierPrice.OriginalPrice + CostCodePrice) * CostCodePercentageMultiplier;
            calculation.BasePrice = (promotionTierPrice.OriginalPrice + CostCodePrice) * CostCodePercentageMultiplier;
            calculation.PercentageMultiplier = promotionPercentageMultipler;
            calculation.PercentageMultiplierWithoutPromotion = percentageMultipler;
            calculation.ContactPrice = contactTierPrice.OriginalPrice;
            calculation.ContactPercentageMultiplier = contactPercentageMultiplier;
            calculation.RoundingDigits = promotionTierPrice.Rounding.NullIf(0) ?? tierPrice.Rounding.NullIf(0) ?? 2;
            calculation.VatGroupID = contactTierPrice.VatGroupID == 0 ? tierPrice.VatGroupID : contactTierPrice.VatGroupID;
            calculation.VatPercentage = contactTierPrice.VatGroupID == 0 ? tierPrice.VatPercentage : contactTierPrice.VatPercentage;
            calculation.IsInclusive = IsInclusive;
            calculation.From = from;
            calculation.To = to;
            calculation.MarginPrice = promotionTierPrice.MarginPrice;
            calculation.PromoMarginPrice = promotionTierPrice.PromoMarginPrice;
            calculation.DiscountFromContact = ContactDiscounts.HasDiscount;
            calculation.MergeWith(promotionTierPrice, ContactDiscounts);
            return calculation;
        }

        private static bool IsValidPriceDate(DateTime d) {
            return d != default && !d.IsBaseDate();
        }
    }
}