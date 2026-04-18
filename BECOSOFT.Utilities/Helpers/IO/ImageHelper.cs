using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions.IO;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;

namespace BECOSOFT.Utilities.Helpers.IO {
    /// <summary>
    /// Helper class for images
    /// </summary>
    public static class ImageHelper {
        /// <summary>
        /// Creates a <see cref="Bitmap"/> from the provided <see cref="path"/>.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Bitmap BitmapFromFile(string path) {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                using (var img = new Bitmap(stream)) {
                    return new Bitmap(img);
                }
            }
        }

        /// <summary>
        /// Method to "convert" an Image object into a byte array, formatted in PNG file format, which 
        /// provides lossless compression. This can be used together with the GetImageFromByteArray() 
        /// method to provide a kind of serialization / deserialization. 
        /// </summary>
        /// <param name="image">Image object, must be convertable to PNG format</param>
        /// <returns>byte array image of a PNG file containing the image</returns>
        public static byte[] CopyImageToByteArray(Image image) {
            using (var memoryStream = new MemoryStream()) {
                image.Save(memoryStream, ImageFormat.Jpeg);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Method that uses the ImageConverter object in .Net Framework to convert a byte array, 
        /// presumably containing a JPEG or PNG file image, into a Bitmap object, which can also be 
        /// used as an Image object.
        /// </summary>
        /// <param name="byteArray">byte array containing JPEG or PNG file image or similar</param>
        /// <param name="setDefaultResolution">(Optional) Set the DPI to 96 (defaults to false)</param>
        /// <returns>Bitmap object if it works, else an empty image (1x1) is returned</returns>
        public static Bitmap GetImageFromByteArray(byte[] byteArray, bool setDefaultResolution = false) {
            var imageConverter = new ImageConverter();
            Bitmap bm;
            try {
                bm = (Bitmap) imageConverter.ConvertFrom(byteArray);
            } catch (NotSupportedException) {
                bm = GetEmptyImage(1, 1);
            } catch (ArgumentException) {
                bm = GetEmptyImage(1, 1);
            }

            if (bm == null) {
                return GetEmptyImage(1, 1);
            }

            if (setDefaultResolution) {
                bm.SetResolution(96, 96);
            } else if ((Math.Abs(bm.HorizontalResolution - (int) bm.HorizontalResolution) > 0.01 || Math.Abs(bm.VerticalResolution - (int) bm.VerticalResolution) > 0.01)) {
                // Correct a strange glitch that has been observed in the test program when converting 
                //  from a PNG file image created by CopyImageToByteArray() - the dpi value "drifts" 
                //  slightly away from the nominal integer value
                bm.SetResolution((int) (bm.HorizontalResolution + 0.5f), (int) (bm.VerticalResolution + 0.5f));
            }

            return bm;
        }

        public static Bitmap GetEmptyImage(int width, int height) {
            using (var bm = new Bitmap(width, height)) {
                using (var mem = new MemoryStream()) {
                    bm.Save(mem, ImageFormat.Png);
                    return new Bitmap(mem);
                }
            }
        }

        /// <summary>
        /// Methods that scales an image (given as a byte-array) and returns it as a byte-array
        /// </summary>
        /// <param name="imageData">The image-data</param>
        /// <param name="width">The width</param>
        /// <param name="height">The height</param>
        /// <returns>The scaled image as a byte-array</returns>
        public static byte[] ScaleImage(byte[] imageData, int width, int height) {
            var image = GetImageFromByteArray(imageData);
            var size = new Size(width, height);
            var resizedImage = image.Scale(size);
            return CopyImageToByteArray(resizedImage);
        }

        /// <summary>
        /// Method that scales an image (given as a byte-array) when either width or height is bigger than the maximum allowed size.
        /// This returns the scaled image as a byte-array when the original is bigger than the maximum size.
        /// Otherwise it returns the original byte-array.
        /// </summary>
        /// <param name="imageData">The image-data</param>
        /// <param name="maximumSize">The maximum allowed size</param>
        /// <param name="setDefaultResolution">(Optional) Set the DPI to 96 (defaults to false)</param>
        /// <returns>The scaled or original image as a byte-array</returns>
        public static byte[] ScaleImage(byte[] imageData, int maximumSize, bool setDefaultResolution = false) {
            var image = GetImageFromByteArray(imageData, setDefaultResolution);
            var longestSize = image.Height > image.Width ? image.Height : image.Width;

            //Rescale when the size exceeds the limit
            if (longestSize > maximumSize) {
                var resizeRatio = (decimal) maximumSize / longestSize;
                var newWidth = (image.Width * resizeRatio).To<int>();
                var newHeight = (image.Height * resizeRatio).To<int>();
                var size = new Size(newWidth, newHeight);
                image = image.Scale(size);
            }

            return image.ToByteArray();
        }

        public static byte[] RotateImage(byte[] imageData, RotateFlipType rotateFlipType) {
            var image = GetImageFromByteArray(imageData);
            image.RotateFlip(rotateFlipType);
            return image.ToByteArray();
        }

        public static Size GetSizeFromImageUrl(Uri url) {
            var response = new HttpClient().GetAsync(url).Result;
            if (response.IsSuccessStatusCode) {
                var content = response.Content.ReadAsByteArrayAsync().Result;

                var image = GetImageFromByteArray(content);

                return new Size(image.Width, image.Height);
            }

            return new Size(0, 0);
        }

        public static RotateFlipType? ConvertToRotateFlipType(int rotation) {
            switch (rotation) {
                case 90:
                    return RotateFlipType.Rotate90FlipNone;
                case -90:
                    return RotateFlipType.Rotate270FlipNone;
                case 180:
                    return RotateFlipType.Rotate180FlipNone;
                case -180:
                    return RotateFlipType.Rotate180FlipNone;
                case 270:
                    return RotateFlipType.Rotate270FlipNone;
                case -270:
                    return RotateFlipType.Rotate90FlipNone;
                default:
                    return null;
            }
        }
    }
}
