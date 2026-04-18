using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BECOSOFT.Utilities.IO {
    /// <summary>
    /// Wrapper class providing faster <see cref="Bitmap"/> pixel operations. Uses <see cref="IDisposable"/>, so bits are locked (<see cref="LockableBitmap.LockBits"/>) in the constructor and unlocked (<see cref="LockableBitmap.UnlockBits"/>) in the <see cref="IDisposable.Dispose"/> function.
    /// </summary>
    public class DisposableLockableBitmap : LockableBitmap, IDisposable {
        public DisposableLockableBitmap(Bitmap source)
            : base(source) {
            LockBits();
        }

        public void Dispose() {
            UnlockBits();
        }
    }

    /// <summary>
    /// Wrapper class providing faster <see cref="Bitmap"/> pixel operations.
    /// </summary>
    public class LockableBitmap {
        private readonly Bitmap _source;
        private IntPtr _pointer;
        private BitmapData _bitmapData;

        public byte[] Pixels { get; set; }
        public int Depth { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public LockableBitmap(Bitmap source) {
            _source = source;
        }

        /// <summary>
        /// Lock bitmap data
        /// </summary>
        public void LockBits() {
            // Get width and height of bitmap
            Width = _source.Width;
            Height = _source.Height;

            // get total locked pixels count
            var pixelCount = Width * Height;

            // Create rectangle to lock
            var rect = new Rectangle(0, 0, Width, Height);

            // get source bitmap pixel format size
            Depth = Image.GetPixelFormatSize(_source.PixelFormat);

            // Check if bpp (Bits Per Pixel) is 8, 24, or 32
            if (Depth != 8 && Depth != 24 && Depth != 32) {
                throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
            }

            // Lock bitmap and return bitmap data
            _bitmapData = _source.LockBits(rect, ImageLockMode.ReadWrite,
                                           _source.PixelFormat);

            // create byte array to copy pixel values
            var step = Depth / 8;
            Pixels = new byte[pixelCount * step];
            _pointer = _bitmapData.Scan0;

            // Copy data from pointer to array
            Marshal.Copy(_pointer, Pixels, 0, Pixels.Length);
        }

        /// <summary>
        /// Unlock bitmap data
        /// </summary>
        public void UnlockBits() {
            // Copy data from byte array to pointer
            Marshal.Copy(Pixels, 0, _pointer, Pixels.Length);

            // Unlock bitmap data
            _source.UnlockBits(_bitmapData);
        }

        /// <summary>
        /// Get the color of the specified pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Color GetPixel(int x, int y) {
            var clr = Color.Empty;

            var i = GetStartIndex(x, y);

            switch (Depth) {
                // For 32 bpp get Red, Green, Blue and Alpha
                case 32: {
                    var b = Pixels[i];
                    var g = Pixels[i + 1];
                    var r = Pixels[i + 2];
                    var a = Pixels[i + 3]; // a
                    clr = Color.FromArgb(a, r, g, b);
                    break;
                }
                // For 24 bpp get Red, Green and Blue
                case 24: {
                    var b = Pixels[i];
                    var g = Pixels[i + 1];
                    var r = Pixels[i + 2];
                    clr = Color.FromArgb(r, g, b);
                    break;
                }
                // For 8 bpp get color value (Red, Green and Blue values are the same)
                case 8: {
                    var c = Pixels[i];
                    clr = Color.FromArgb(c, c, c);
                    break;
                }
            }
            return clr;
        }

        /// <summary>
        /// Set the color of the specified pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public void SetPixel(int x, int y, Color color) {
            var i = GetStartIndex(x, y);

            switch (Depth) {
                // For 32 bpp set Red, Green, Blue and Alpha
                case 32:
                    Pixels[i] = color.B;
                    Pixels[i + 1] = color.G;
                    Pixels[i + 2] = color.R;
                    Pixels[i + 3] = color.A;
                    break;
                // For 24 bpp set Red, Green and Blue
                case 24:
                    Pixels[i] = color.B;
                    Pixels[i + 1] = color.G;
                    Pixels[i + 2] = color.R;
                    break;
                // For 8 bpp set color value (Red, Green and Blue values are the same)
                case 8:
                    Pixels[i] = color.B;
                    break;
            }
        }

        private int GetStartIndex(int x, int y) {
            // Get color components count
            var cCount = Depth / 8;

            // Get start index of the specified pixel
            var i = ((y * Width) + x) * cCount;

            if (i > Pixels.Length - cCount) {
                throw new IndexOutOfRangeException();
            }
            return i;
        }
    }
}