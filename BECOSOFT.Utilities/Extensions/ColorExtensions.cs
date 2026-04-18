using BECOSOFT.Utilities.Converters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace BECOSOFT.Utilities.Extensions {
    /// <summary>
    /// Extension methods for creating and converting colors
    /// </summary>
    public static class ColorExtensions {
        private static readonly Random RandomGenerator = new Random();

        private static readonly List<Color> Colors = new List<Color> {
            Color.FromArgb(203, 144, 77),
            Color.FromArgb(81, 163, 163),
            Color.FromArgb(223, 204, 116),
            Color.FromArgb(62, 98, 89),
            Color.FromArgb(232, 144, 5),
            Color.FromArgb(91, 130, 102),
            Color.FromArgb(195, 233, 145),
            Color.FromArgb(117, 72, 94),
            //Color.FromArgb(236, 117, 5),
            Color.FromArgb(216, 74, 5),
            //Color.FromArgb(244, 43, 3),
            //Color.FromArgb(231, 14, 2),
            Color.FromArgb(220, 90, 120),
            Color.FromArgb(150, 160, 75),
            Color.FromArgb(41, 73, 54),
            Color.FromArgb(174, 246, 199)
        };

        /// <summary>
        /// Generates a random color
        /// </summary>
        /// <param name="limited">(Optional) Value indicating whether the color should be picked from a limited list</param>
        /// <returns>The random color</returns>
        public static Color GenerateRandomColor(bool limited = true) {
            return limited ? Colors[RandomGenerator.Next(Colors.Count)] : Color.FromArgb(RandomGenerator.Next(256), RandomGenerator.Next(256), RandomGenerator.Next(256));
        }

        /// <summary>
        /// Compares two colors based on their Argb value since Color.Equals also checks on named color and other color properties.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="otherColor"></param>
        /// <returns></returns>
        public static bool IsEqualTo(this Color color, Color otherColor) {
            return color.ToArgb() == otherColor.ToArgb();
        }

        /// <summary>
        /// Converts a color to a javascript-RGBA-color
        /// rgba(R, G, B, A)
        /// </summary>
        /// <param name="color">The color to convert</param>
        /// <param name="alpha">The alpha value</param>
        /// <returns>The javascript-RGBA-color</returns>
        public static string ToJavaScriptRgba(this Color color, double alpha = 1.0) {
            return $"rgba({color.R}, {color.G}, {color.B}, {alpha.ToString(CultureInfo.InvariantCulture)})";
        }

        /// <summary>
        /// Gets the color based on the day of the week
        /// </summary>
        /// <param name="dayOfWeek">The day of the week</param>
        /// <returns>The color of that day</returns>
        public static Color GetColor(DayOfWeek dayOfWeek) {
            switch (dayOfWeek) {
                case DayOfWeek.Sunday:
                    return Color.FromArgb(139, 0, 255);
                case DayOfWeek.Monday:
                    return Color.FromArgb(255, 0, 0);
                case DayOfWeek.Tuesday:
                    return Color.FromArgb(255, 127, 0);
                case DayOfWeek.Wednesday:
                    return Color.FromArgb(255, 255, 0);
                case DayOfWeek.Thursday:
                    return Color.FromArgb(0, 255, 0);
                case DayOfWeek.Friday:
                    return Color.FromArgb(0, 0, 255);
                case DayOfWeek.Saturday:
                    return Color.FromArgb(75, 0, 130);
                default:
                    return Color.Empty;
            }
        }

        /// <summary>
        /// Get a color from a predefinied color-array
        /// </summary>
        /// <param name="index">The index of the color</param>
        /// <returns>The color</returns>
        public static Color GetLimitedColor(int index) {
            if (index < 0 || index >= Colors.Count) {
                return Colors[0];
            }
            return Colors[index];
        }

        /// <summary>
        /// Gets the best contrast color (black or white) based on a color
        /// Source: https://stackoverflow.com/a/1855903/4182837
        /// </summary>
        /// <param name="color">The background-color</param>
        /// <returns>The contrast color</returns>
        public static Color GetContrastColor(this Color color) {
            // Counting the perceptive luminance - human eye favors green color... 
            var perceptiveLuminance = 1 - (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;

            var rgbValue = perceptiveLuminance < 0.5 ? 0 : 255;

            return Color.FromArgb(rgbValue, rgbValue, rgbValue);
        }
    }

    public static class ColorHelpers {
        public static Color Parse(string value, Color defaultColor = default) {
            if (value.IsNullOrWhiteSpace()) {
                return defaultColor;
            }
            try {
                var colorValue = value.RemoveWhitespace().RemoveControlCharacters();
                if (IsValidHexColor(colorValue, false)) {
                    var color = ColorTranslator.FromHtml(colorValue);
                    return color;
                }
                try {
                    return Color.FromName(value);
                } catch (Exception) {
                    return defaultColor;
                }
            } catch (Exception) {
                return defaultColor;
            }
        }

        /// <summary>
        /// Checks whether the provided <paramref name="value"/> is a valid Hex color. Format: #RRGGBB (Example: #FFAA88).
        /// <paramref name="allowTransparancy"/> allows the Hex color to have the alpha component. Format: #RRGGBBAA (Example: #FFAA8811).
        /// </summary>
        /// <param name="value"></param>
        /// <param name="allowTransparancy"></param>
        /// <returns></returns>
        public static bool IsValidHexColor(string value, bool allowTransparancy = true) {
            if (value.IsNullOrWhiteSpace()) { return false; }
            if (value.Length < 3) { return false; }
            var temp = (value.StartsWith("#") ? value.Substring(1) : value).ToUpper();
            if (temp.Length != 3 && temp.Length != 6 && (!allowTransparancy || temp.Length != 8)) { return false; }
            return temp.All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F'));
        }


        /// <summary>
        /// Generates a list of <see cref="Color"/> that contain both <see cref="start"/> and <see cref="end"/> and extra colors (defined by <see cref="numberOfColors"/>) that lie between the two colors.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="numberOfColors"></param>
        /// <returns></returns>
        public static List<Color> Generate(Color start, Color end, int numberOfColors) {
            if (start == end) {
                return new List<Color> { start };
            }
            if (numberOfColors <= 2) {
                return new List<Color> { start, end };
            }
            var result = new List<Color>(numberOfColors);

            ColorToHSV(start, out var startHue, out var startSaturation, out var startValue);
            var currentHue = startHue;
            var currentSaturation = startSaturation;
            var currentValue = startValue;
            ColorToHSV(end, out var endHue, out var endSaturation, out var endValue);
            var dh = Math.Abs(endHue - startHue);
            var ds = Math.Abs(endSaturation - currentSaturation);
            var dv = Math.Abs(endValue - currentValue);
            if (endHue == 0) { endHue = 360; }
            var intervalS = ds / numberOfColors;
            var intervalV = dv / numberOfColors;

            double alpha;
            int multiplier;
            if (startHue > endHue) {
                alpha = 1;
                multiplier = -1;
            } else {
                alpha = 0.0;
                multiplier = 1;
            }
            for (var i = 0; i < numberOfColors; i++) {

                var color = ColorFromHSV(currentHue, currentSaturation, currentValue);
                alpha += (1.0 / (numberOfColors - 1)) * multiplier;
                currentHue = startHue + (alpha * dh);
                currentSaturation += intervalS;
                currentValue += intervalV;
                result.Add(color);
            }
            
            return result;
        }

        private static void ColorToHSV(Color color, out double hue, out double saturation, out double value) {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        private static Color ColorFromHSV(double hue, double saturation, double value) {
            var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            var f = hue / 60 - Math.Floor(hue / 60);

            value *= 255;
            var v = Convert.ToInt32(value);
            var p = Convert.ToInt32(value * (1 - saturation));
            var q = Convert.ToInt32(value * (1 - f * saturation));
            var t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            switch (hi) {
                case 0:
                    return Color.FromArgb(255, v, t, p);
                case 1:
                    return Color.FromArgb(255, q, v, p);
                case 2:
                    return Color.FromArgb(255, p, v, t);
                case 3:
                    return Color.FromArgb(255, p, q, v);
                case 4:
                    return Color.FromArgb(255, t, p, v);
                default:
                    return Color.FromArgb(255, v, p, q);
            }
        }
    }
}