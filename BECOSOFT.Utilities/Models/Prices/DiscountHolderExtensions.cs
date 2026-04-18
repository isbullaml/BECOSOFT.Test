using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Extensions.Numeric;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Models.Prices {
    public static class DiscountHolderExtensions {
        private static readonly IReadOnlyList<int> IndexEnumerable = Enumerable.Range(BaseDiscountHolder.MinDiscountIndex, BaseDiscountHolder.MaxDiscountIndex).ToList();
        public static DiscountHolder ToDiscountHolder(this IDiscountHolder discountHolder) {
            var result = new DiscountHolder();
            foreach (var index in result.GetIndices()) {
                result.SetDiscount(index, discountHolder.GetDiscount(index));
            }
            return result;
        }


        /// <summary>
        /// Returns a list of indices, from <see cref="DiscountHolder.MinDiscountIndex"/> to <see cref="DiscountHolder.MaxDiscountIndex"/>.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<int> GetIndices(this IDiscountHolder _) => IndexEnumerable;

        /// <summary>
        /// All discounts (1-5) multiplied with each other.
        /// </summary>
        /// <param name="discountHolder"></param>
        /// <returns></returns>
        public static decimal GetDiscountMultiplier(this IDiscountHolder discountHolder) {
            return (1m - (discountHolder.Discount1 / 100m))
                   * (1m - (discountHolder.Discount2 / 100m))
                   * (1m - (discountHolder.Discount3 / 100m))
                   * (1m - (discountHolder.Discount4 / 100m))
                   * (1m - (discountHolder.Discount5 / 100m));
        }

        /// <summary>
        /// All discounts (1-5) multiplied with each other.
        /// </summary>
        /// <param name="discountHolder"></param>
        /// <returns></returns>
        public static bool GetHasDiscount(this IDiscountHolder discountHolder) {
            return discountHolder.Discount1 != 0
                   || discountHolder.Discount2 != 0
                   || discountHolder.Discount3 != 0
                   || discountHolder.Discount4 != 0
                   || discountHolder.Discount5 != 0;
        }

        /// <summary>
        /// Gets the discount percentage by <paramref name="discountNumber"/> (1 to 5).
        /// </summary>
        /// <param name="discountHolder"></param>
        /// <param name="discountNumber">Index of the discount</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static decimal GetDiscount(this IDiscountHolder discountHolder, int discountNumber) {
            switch (discountNumber) {
                case 1:
                    return discountHolder.Discount1;
                case 2:
                    return discountHolder.Discount2;
                case 3:
                    return discountHolder.Discount3;
                case 4:
                    return discountHolder.Discount4;
                case 5:
                    return discountHolder.Discount5;
                default:
                    throw new ArgumentOutOfRangeException(nameof(discountNumber));
            }
        }


        /// <summary>
        /// Sets the <paramref name="discount"/> percentage by <paramref name="discountNumber"/> (1 to 5).
        /// </summary>
        /// <param name="discountHolder"></param>
        /// <param name="discountNumber">Index of the discount</param>
        /// <param name="discount">Value of the discount</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static void SetDiscount(this IDiscountHolder discountHolder, int discountNumber, decimal discount) {
            switch (discountNumber) {
                case 1:
                    discountHolder.Discount1 = discount;
                    break;
                case 2:
                    discountHolder.Discount2 = discount;
                    break;
                case 3:
                    discountHolder.Discount3 = discount;
                    break;
                case 4:
                    discountHolder.Discount4 = discount;
                    break;
                case 5:
                    discountHolder.Discount5 = discount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(discountNumber));
            }
        }


        /// <summary>
        /// Merges all discount holders (<paramref name="discountHolders"/>) with the <paramref name="currentDiscountHolder"/> and returns the merged result.
        /// </summary>
        /// <param name="currentDiscountHolder"></param>
        /// <param name="discountHolders"></param>
        /// <returns></returns>
        public static DiscountHolder MergedWith(this IDiscountHolder currentDiscountHolder, params IDiscountHolder[] discountHolders) {
            var result = ToDiscountHolder(currentDiscountHolder);
            if (discountHolders.IsEmpty()) {
                return result;
            }
            foreach (var discountHolder in discountHolders) {
                result = MergeDiscounts(result, discountHolder);
            }
            return result;
        }

        /// <summary>
        /// Merges all discount holders (<paramref name="discountHolders"/>) with the <paramref name="currentDiscountHolder"/>.
        /// The <paramref name="currentDiscountHolder"/> will be updated with the new discounts
        /// </summary>
        /// <param name="currentDiscountHolder"></param>
        /// <param name="discountHolders"></param>
        public static void MergeWith(this IDiscountHolder currentDiscountHolder, params IDiscountHolder[] discountHolders) {
            if (discountHolders.IsEmpty()) { return; }
            var result = currentDiscountHolder.ToDiscountHolder();
            foreach (var discountHolder in discountHolders) {
                result = MergeDiscounts(result, discountHolder);
            }
            foreach (var index in result.GetIndices()) {
                currentDiscountHolder.SetDiscount(index, result.GetDiscount(index));
            }
        }

        private static Dictionary<int, decimal> GetDiscountDictionary(this IDiscountHolder discountHolder, bool excludeZeroDiscount = false) {
            var result = new Dictionary<int, decimal>(IndexEnumerable.Count);
            for (var index = BaseDiscountHolder.MinDiscountIndex; index <= BaseDiscountHolder.MaxDiscountIndex; index++) {
                var discount = discountHolder.GetDiscount(index);
                if (excludeZeroDiscount && discount == 0) { continue; }
                result[index] = discount;
            }
            return result;
        }

        private static DiscountHolder MergeDiscounts(IDiscountHolder left, IDiscountHolder right) {
            var result = new DiscountHolder();
            if (left.HasDiscount && !right.HasDiscount) {
                return left.ToDiscountHolder();
            }
            if (!left.HasDiscount && right.HasDiscount) {
                return right.ToDiscountHolder();
            }
            var leftDiscounts = left.GetDiscountDictionary(true);
            var rightDiscounts = right.GetDiscountDictionary(true);
            var numberOfLeftDiscounts = leftDiscounts.Count;
            var numberOfRightDiscounts = rightDiscounts.Count;
            if (numberOfLeftDiscounts + numberOfRightDiscounts <= BaseDiscountHolder.MaxDiscountIndex) {
                var currentDiscountIndex = 1;
                foreach (var discount in leftDiscounts.OrderBy(l => l.Key)) {
                    result.SetDiscount(currentDiscountIndex, discount.Value);
                    currentDiscountIndex++;
                }
                foreach (var discount in rightDiscounts.OrderBy(r => r.Key)) {
                    result.SetDiscount(currentDiscountIndex, discount.Value);
                    currentDiscountIndex++;
                }
            } else {
                for (var i = BaseDiscountHolder.MinDiscountIndex; i <= BaseDiscountHolder.MaxDiscountIndex; i++) {
                    var discounts = new List<decimal>(2);
                    var tempDisc = leftDiscounts.TryGetValueWithDefault(i);
                    if (tempDisc != 0) {
                        discounts.Add(tempDisc);
                    }
                    tempDisc = rightDiscounts.TryGetValueWithDefault(i);
                    if (tempDisc != 0) {
                        discounts.Add(tempDisc);
                    }
                    decimal newDiscount;
                    if (discounts.Count == 0) {
                        newDiscount = 0m;
                    } else if (discounts.Count == 1) {
                        newDiscount = discounts[0];
                    } else {
                        // EXP(SUM(LOG(ABS(1 - (Percentage / 100)))))
                        var multipliers = discounts.Select(d => Math.Abs(1m - (d / 100m))).ToList();
                        var multiplierDoubles = multipliers.Select(d => d.ToDouble()).ToList();
                        var logValues = multiplierDoubles.Select(d => Math.Log(d)).ToList();
                        var summedMultipliers = logValues.Sum();
                        var expValue = Math.Exp(summedMultipliers);
                        newDiscount = ((1m - expValue.ToDecimal()) * 100m).RoundTo(2);
                    }
                    result.SetDiscount(i, newDiscount);
                }
            }
            return result;
        }
    }
}