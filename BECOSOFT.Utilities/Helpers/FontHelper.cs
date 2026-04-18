using System;
using System.Drawing;

namespace BECOSOFT.Utilities.Helpers {
    /// <summary>
    /// Helper class for fonts
    /// </summary>
    public static class FontHelper {
        /// <summary>
        /// Checks if a font is installed
        /// </summary>
        /// <param name="fontName">The name of the font to check</param>
        /// <returns>True if the font is installed, false if not</returns>
        public static bool IsFontInstalled(string fontName) {
            var installed = IsFontInstalled(fontName, FontStyle.Regular);
            if (!installed) { installed = IsFontInstalled(fontName, FontStyle.Bold); }
            if (!installed) { installed = IsFontInstalled(fontName, FontStyle.Italic); }

            return installed;
        }

        /// <summary>
        /// Checks if a font is installed with a specific style
        /// </summary>
        /// <param name="fontName">The name of the font to check</param>
        /// <param name="style">The style of the font</param>
        /// <returns>True if the font is installed, false if not</returns>
        public static bool IsFontInstalled(string fontName, FontStyle style) {
            var installed = false;
            const float emSize = 8.0f;

            try {
                using (var testFont = new Font(fontName, emSize, style)) {
                    installed = 0 == string.Compare(fontName, testFont.Name, StringComparison.InvariantCultureIgnoreCase);
                }
            } catch {
            }

            return installed;
        }
    }
}
