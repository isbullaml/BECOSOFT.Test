using System.Drawing;

namespace BECOSOFT.Utilities.Extensions {
    /// <summary>
    /// Extensions for a <see cref="Rectangle"/>
    /// </summary>
    public static class RectangleExtensions {
        /// <summary>
        /// Gets the center of a <see cref="Rectangle"/>
        /// </summary>
        /// <param name="rectangle">The rectangle</param>
        /// <returns>The center of the rectangle</returns>
        public static Point Center(this Rectangle rectangle) {
            return new Point(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2);
        }
    }
}
