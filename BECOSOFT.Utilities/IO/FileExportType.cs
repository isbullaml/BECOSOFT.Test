using System;

namespace BECOSOFT.Utilities.IO {
    /// <summary>
    /// Types of file-export
    /// </summary>
    public enum FileExportType {
        /// <summary>
        /// .txt
        /// </summary>
        Text = 1,
        /// <summary>
        /// .csv
        /// </summary>
        CommaSeparatedValues = 2,
        /// <summary>
        /// .xls
        /// </summary>
        Excel2003 = 3,
        /// <summary>
        /// .xlsx
        /// </summary>
        Excel = 4,
        /// <summary>
        /// .pdf
        /// </summary>
        PortableDocumentFormat = 5,
        /// <summary>
        /// .xlsm
        /// </summary>
        ExcelWithMacros = 6,
    }

    /// <summary>
    /// Helper class for <see cref="FileExportType"/>
    /// </summary>
    public static class FileExportTypeExtensions {
        /// <summary>
        /// Gets the extension for a <see cref="FileExportType"/>
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>The extension</returns>
        public static string GetExtension(this FileExportType type) {
            switch (type) {
                case FileExportType.Text:
                    return ".txt";
                case FileExportType.CommaSeparatedValues:
                    return ".csv";
                case FileExportType.Excel2003:
                    return ".xls";
                case FileExportType.Excel:
                    return ".xlsx";
                case FileExportType.PortableDocumentFormat:
                    return ".pdf";
                case FileExportType.ExcelWithMacros:
                    return ".xlsm";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// Returns the MIME type based on the file type.
        /// </summary>
        /// <param name="type">The file type</param>
        /// <returns>The MIME type</returns>
        public static string GetMediaTypeName(this FileExportType type) {
            return MimeTypeMap.GetMimeType(type.GetExtension());
        }
    }
}