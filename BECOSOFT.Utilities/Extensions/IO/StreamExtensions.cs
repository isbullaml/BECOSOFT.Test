using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace BECOSOFT.Utilities.Extensions.IO {
    public static class StreamExtensions {
        // https://web.archive.org/web/20090302032444/http://www.mikekunz.com/image_file_header.html
        private static readonly Dictionary<string, ImageFormat> ImageFormatHeaders = new Dictionary<string, ImageFormat> {
            { "FFD8", ImageFormat.Jpeg},
            { "424D", ImageFormat.Bmp },
            { "474946", ImageFormat.Gif },
            { "89504E470D0A1A0A", ImageFormat.Png },
            { "4949002A", ImageFormat.Tiff },
            { "49492A00", ImageFormat.Tiff },
            { "4D4D2A00", ImageFormat.Tiff },
            { "4D4D002A", ImageFormat.Tiff },

        };
        public static bool IsOpen(this Stream stream) {
            if (stream == null) {
                return false;
            }
            return stream.CanRead || stream.CanSeek || stream.CanWrite;
        }

        public static bool IsImage(this Stream stream) {
            ImageFormat format;
            return stream.IsImage(out format);
        }

        public static bool IsImage(this Stream stream, out ImageFormat format) {
            if (stream.CanSeek) {
                stream.Seek(0, SeekOrigin.Begin);
            }
            try {
                var builder = new StringBuilder();
                var largestHeader = ImageFormatHeaders.Max(i => i.Key.Length / 2);
                for (var i = 0; i < largestHeader; i++) {
                    var bit = stream.ReadByte().ToString("X2");
                    builder.Append(bit);
                    var headerString = builder.ToString();
                    var isImage = ImageFormatHeaders.TryGetValue(headerString, out format);
                    if (isImage) {
                        return true;
                    }
                }
                format = null;
                return false;
            } finally {
                if (stream.CanSeek) {
                    stream.Seek(0, SeekOrigin.Begin);
                }
            }
        }
    }
}