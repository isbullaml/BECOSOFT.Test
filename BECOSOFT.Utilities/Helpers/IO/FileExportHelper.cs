using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.IO;
using BECOSOFT.Utilities.Models.IO;
using OfficeOpenXml;
using System;
using System.IO;

namespace BECOSOFT.Utilities.Helpers.IO {
    public static class FileExportHelper {
        public static ContentFileResult Save(ContentFileExportParameters exportParameters) {
            var result = new ContentFileResult();
            if (!exportParameters.Files.HasAny()) {
                return result;
            }
            if (!Directory.Exists(exportParameters.Directory)) {
                Directory.CreateDirectory(exportParameters.Directory);
            }
            var extension = exportParameters.FileExtension ?? exportParameters.FileType.GetExtension();
            foreach (var file in exportParameters.Files) {
                var fileName = GetFileName(file, extension);
                var tempPath = GetFullPath(exportParameters, fileName);

                WriteToPath(exportParameters, tempPath, file);

                result.Exports.Add(new ContentFileData {
                    File = file,
                    FullPath = tempPath
                });
            }

            return result;
        }

        private static string GetFullPath(ContentFileExportParameters exportParameters, string fileName) {
            var tempPath = fileName;
            if (!exportParameters.Directory.IsNullOrWhiteSpace()) {
                tempPath = Path.Combine(exportParameters.Directory, tempPath);
            }

            return tempPath;
        }

        private static string GetFileName(ContentFile file, string extension) {
            var fileName = file.FileName ?? Path.GetRandomFileName();
            fileName = FileHelpers.CleanFileName(fileName);
            fileName = Path.ChangeExtension(fileName, extension);
            return fileName;
        }

        private static void WriteToPath(ContentFileExportParameters exportParameters, string tempPath, ContentFile file) {
            switch (exportParameters.FileType) {
                case FileExportType.Text:
                case FileExportType.CommaSeparatedValues:
                    WriteAsSeparatedValueFile(exportParameters, tempPath, file);
                    break;
                case FileExportType.Excel:
                    WriteAsExcelFile(exportParameters, tempPath, file);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void WriteAsExcelFile(ContentFileExportParameters exportParameters, string tempPath, ContentFile file) {
            var fileInfo = new FileInfo(tempPath);
            using (var excel = new ExcelPackage(fileInfo)) {
                var worksheet = excel.Workbook.Worksheets.Add("Export");
                var rowOffset = 1;
                if (exportParameters.IncludeHeaders) {
                    for (var colIndex = 0; colIndex < file.Headers.Count; colIndex++) {
                        var header = file.Headers[colIndex];
                        worksheet.SetValue(1, colIndex + 1, header.Name);
                    }

                    worksheet.Cells[1, 1, 1, file.Headers.Count].Style.Font.Bold = true;
                    rowOffset = 2;
                }
                for (var rowIndex = 0; rowIndex < file.Lines.Count; rowIndex++) {
                    var row = rowIndex + rowOffset;
                    var line = file.Lines[rowIndex];
                    for (var colIndex = 0; colIndex < line.Values.Count; colIndex++) {
                        var cell = line.Values[colIndex];
                        worksheet.SetValue(row, colIndex + 1, cell.Value);
                    }
                }

                if (exportParameters.AutoFitColumns) {
                    worksheet.Cells.AutoFitColumns();
                }

                excel.Save();
            }
        }

        private static void WriteAsSeparatedValueFile(ContentFileExportParameters exportParameters, string tempPath, ContentFile file) {
            using (var sw = new StreamWriter(tempPath, false, exportParameters.Encoding)) {
                var writeOptions = new ContentFileWriteOptions {
                    IncludeHeaders = exportParameters.IncludeHeaders,
                    PreHeader = exportParameters.PreHeader,
                    Separator = exportParameters.Separator,
                };
                file.WriteTo(sw, writeOptions);
            }
        }
    }
}