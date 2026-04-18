using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace BECOSOFT.Utilities.Extensions {
    public static class GraphicsExtensions {
        public static void SetHighestQuality(this Graphics graphics) {
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
        }
    }
}