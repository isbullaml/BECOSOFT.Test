using BECOSOFT.Utilities.IO;
using NLog;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

// Inspiration: https://www.codeproject.com/Tips/240428/Work-with-Bitmaps-Faster-in-Csharp-3

namespace BECOSOFT.Utilities.Extensions.IO {
    /// <summary>
    /// Extensions for <see cref="Image"/>
    /// </summary>
    public static class ImageExtensions {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Copies the image
        /// </summary>
        /// <param name="image">The image to copy</param>
        /// <returns>The copied image</returns>
        public static Bitmap Copy(this Image image) {
            var img = new Bitmap(image);
            img.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            return img;
        }

        /// <summary>
        /// Scales an image to a size
        /// </summary>
        /// <param name="image">The image to scale</param>
        /// <param name="size">The wanted size</param>
        /// <param name="setResolution">Set the resolution of the original image on the new image.</param>
        /// <returns>The scaled image</returns>
        public static Bitmap Scale(this Image image, Size size, bool setResolution = true) {
            var ratioX = (double)size.Width / image.Width;
            var ratioY = (double)size.Height / image.Height;
            var ratio = Math.Min(ratioX, ratioY);
            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            if (setResolution) {
                newImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            }
            using (var graphics = Graphics.FromImage(newImage)) {
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

        /// <summary>
        /// Converts an image to a grey scale representation of the original image.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static Bitmap ToGreyScale(this Image image) {
            // https://stackoverflow.com/questions/2265910/convert-an-image-to-grayscale
            var originalImageBitmap = image as Bitmap;
            if (originalImageBitmap == null) {
                return null;
            }
            using (var lockableBitmapOriginal = new DisposableLockableBitmap(originalImageBitmap)) {
                var result = new Bitmap(originalImageBitmap.Width, originalImageBitmap.Height);
                using (var lockableBitmapResult = new DisposableLockableBitmap(result)) {
                    for (var y = 0; y < lockableBitmapOriginal.Height; y++) {
                        for (var x = 0; x < lockableBitmapOriginal.Width; x++) {
                            var oc = lockableBitmapOriginal.GetPixel(x, y);
                            var greyScale = (int)((oc.R * 0.3) + (oc.G * 0.59) + (oc.B * 0.11));
                            var newColor = Color.FromArgb(oc.A, greyScale, greyScale, greyScale);
                            lockableBitmapResult.SetPixel(x, y, newColor);
                        }
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Saves an image and opens it
        /// </summary>
        /// <param name="bitmap">The image to save</param>
        /// <param name="name">The name to save the image as</param>
        /// <param name="format">The format of the image</param>
        public static void SaveAndOpen(this Bitmap bitmap, string name, ImageFormat format) {
            var fileName = bitmap.SaveWithFormat(name, format);
            Process.Start(fileName);
        }

        /// <summary>
        /// Saves an image
        /// </summary>
        /// <param name="bitmap">The image to save</param>
        /// <param name="name">The name to save the image as</param>
        /// <param name="format">The format of the image</param>
        /// <param name="overwrite">Overwrites the image if the file already exists</param>
        public static string SaveWithFormat(this Bitmap bitmap, string name, ImageFormat format, bool overwrite = true) {
            var extension = format.GetFileExtension();
            if (!extension.StartsWith(".")) {
                extension = "." + extension;
            }
            var fileName = $"{name}{extension}";
            var file = new FileInfo(fileName);

            if (overwrite && file.Exists) {
                try {
                    File.Delete(file.FullName);
                } catch (Exception e) {
                    Logger.Error(e, $"An error occured while trying to delete file {file.FullName}");
                }
            }

            bitmap.Save(fileName, format);
            return file.Name;
        }

        /// <summary>
        /// Saves an image
        /// </summary>
        /// <param name="bitmap">The image to save</param>
        /// <param name="name">The name to save the image as</param>
        /// <param name="overwrite">Overwrites the image if the file already exists</param>
        public static FileInfo Save(this Bitmap bitmap, string name, bool overwrite) {
            var file = new FileInfo(name);

            if (overwrite && file.Exists) {
                try {
                    File.Delete(file.FullName);
                } catch (Exception e) {
                    Logger.Error(e, $"An error occured while trying to delete file {file.FullName}");
                }
            }

            bitmap.Save(file.FullName);
            return file;
        }

        public static string GetFileExtension(this ImageFormat imageFormat) {
            var extension = GetImageEncoderExtension(imageFormat) ?? GetImageDecoderExtension(imageFormat);
            return extension ?? $".{imageFormat.ToString().ToLower()}";
        }

        /// <summary>
        /// Converts the bitmap to a bite array
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(this Image image) {
            using (var stream = new MemoryStream()) {
                image.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }

        private static string GetImageDecoderExtension(ImageFormat imageFormat) {
            return ImageCodecInfo.GetImageDecoders()
                                 .Where(ie => ie.FormatID == imageFormat.Guid)
                                 .Select(CleanExtensionList)
                                 .FirstOrDefault();
        }

        private static string GetImageEncoderExtension(ImageFormat imageFormat) {
            return ImageCodecInfo.GetImageEncoders()
                                 .Where(ie => ie.FormatID == imageFormat.Guid)
                                 .Select(CleanExtensionList)
                                 .FirstOrDefault();
        }

        private static string CleanExtensionList(ImageCodecInfo ie) {
            return ie.FilenameExtension
                     .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                     .First()
                     .Trim('*')
                     .ToLower();
        }
    }
}
