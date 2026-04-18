using BECOSOFT.Utilities.Extensions;
using System;

namespace BECOSOFT.Utilities.Helpers {
    public static class MaskHelper {
        /// <summary>
        /// Apply a <see cref="mask"/> to a value (<see cref="to"/>).
        /// <para>
        /// Mask  : 0000000
        /// Value : 1123
        /// Result: 0001123 
        /// </para>
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string Apply(string mask, string to) {
            if (mask.IsNullOrWhiteSpace()) {
                return to;
            }
            if (to.IsNullOrWhiteSpace()) {
                return mask;
            }
            var trimmedTo = to.Trim().RemoveControlCharacters();
            if (trimmedTo.Length > mask.Length) {
                throw new ArgumentException(Resources.Mask_ToExceedsMaskLength, nameof(to));
            }
            var remainingMask = mask.Substring(0, mask.Length - trimmedTo.Length);
            return $"{remainingMask}{trimmedTo}";
        }
    }
}
