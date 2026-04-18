using BECOSOFT.Utilities.Extensions.Collections;
using System.Linq;

namespace BECOSOFT.Utilities.Helpers {
    /// <summary>
    /// Class used for filtering invalid characters from a string
    /// </summary>
    public static class StringFilter {
        /// <summary>
        /// Filters a string (only letters, digits and _)
        /// </summary>
        /// <param name="original">The original string</param>
        /// <returns>The filtered string</returns>
        public static string Filter(string original) {
            var charArr = original.ToCharArray();
            var filtered = charArr.Where(character => char.IsLetterOrDigit(character) || character == '_')
                                  .ToList();
            return filtered.HasAny() ? new string(filtered.ToArray()).Trim('_') : string.Empty;
        }
    }
}