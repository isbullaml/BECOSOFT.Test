using BECOSOFT.Utilities.IO;
using System;
using System.IO;

namespace BECOSOFT.Utilities.Extensions.IO {
    public static class FileInfoExtensions {
        /// <summary>
        /// Returns the name of the <see cref="FileInfo"/> object without extension.
        /// </summary>
        /// <param name="fileInfo">The <see cref="FileInfo"/> object</param>
        /// <returns>Returns the name of the <see cref="FileInfo"/> without extension.</returns>
        public static string GetNameWithoutExtension(this FileInfo fileInfo) {
            var extension = fileInfo.Extension;
            return fileInfo.Name.Substring(0, fileInfo.Name.Length - extension.Length);
        }

        /// <summary>
        /// Checks if the <see cref="FileInfo"/> object has the provided <param name="extension"></param>
        /// </summary>
        /// <param name="fileInfo">The <see cref="FileInfo"/> object</param>
        /// <param name="extension">Extension to check (with or without .)</param>
        /// <returns></returns>
        public static bool HasExtension(this FileInfo fileInfo, string extension) {
            return fileInfo.Extension.ToLower().Contains(extension);
        }

        /// <summary>
        /// Renames the provided file.
        /// </summary>
        /// <param name="fileInfo">The original file</param>
        /// <param name="fileName">The new filename</param>
        public static void Rename(this FileInfo fileInfo, string fileName) {
            var directory = fileInfo.Directory?.FullName ?? "";
            var destinationPath = Path.Combine(directory, fileName);
            fileInfo.MoveTo(destinationPath);
        }


        /// <summary>
        /// Returns a <see cref="byte"/>-array containing the contents of the given <see cref="FileInfo"/>.
        /// </summary>
        /// <param name="fileInfo">The file for which the contents should be retrieved.</param>
        /// <returns>A <see cref="byte"/>-array containing the contents of the given <see cref="FileInfo"/>.</returns>
        public static byte[] ReadBytes(this FileInfo fileInfo) {
            return File.ReadAllBytes(fileInfo.FullName);
        }

        public static bool IsFileExportType(this FileInfo fileInfo, FileExportType type) {
            if (!File.Exists(fileInfo.FullName)) {
                return type.GetExtension().Equals(fileInfo.Extension);
            }
            var signatureBytes = new byte[8];
            using (var fileStream = new FileStream(fileInfo.FullName, FileMode.Open)) {
                var _ = fileStream.Read(signatureBytes, 0, signatureBytes.Length);
            }
            var signature = BitConverter.ToString(signatureBytes);
            switch (type) {
                case FileExportType.Text:
                case FileExportType.CommaSeparatedValues:
                    return true;
                case FileExportType.Excel2003:
                    return signature.Equals("D0-CF-11-E0-A1-B1-1A-E1");
                case FileExportType.Excel:
                case FileExportType.ExcelWithMacros:
                    return signature.Equals("50-4B-03-04-14-00-06-00");
                case FileExportType.PortableDocumentFormat:
                    return signature.StartsWith("25-50-44-46-2D");
                default:
                    return false;
            }
        }
    }
}
