using System.Linq;

namespace BECOSOFT.Utilities.Helpers {
    public static class PercentageHelper {
        public static decimal GetPercentageChange(decimal fromValue, decimal toValue) {
            if (fromValue == 0) { return 0; }
            var diff = toValue - fromValue;
            var percentageDiff = diff / fromValue * 100;
            return percentageDiff;
        }

        /// <summary>
        /// Returns the discount multiplier for the given <see cref="percentages"/>.
        /// Percentage values usually lie between 0 and 100, giving a multiplier of 1 and 0 respectively.
        /// </summary>
        /// <param name="percentages"></param>
        /// <returns></returns>
        public static decimal GetMultiplier(params decimal[] percentages) {
            return percentages.Aggregate(1m, (current, percentage) => current * (1m - percentage / 100m));
        }

        /// <summary>
        /// Converts a <see cref="multiplier"/> (usually between 0 and 1) to a discount percentage (100% and 0% respectively).
        /// </summary>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        public static decimal ToPercentage(decimal multiplier) {
            return (1m - multiplier) * 100;
        }
    }
}