using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.IO;
using BECOSOFT.Utilities.Models.IO;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BECOSOFT.Utilities.Helpers.IO {
    public static class FileImportHelper {
        public static ContentFileResult Import(ContentFileImportParameters importParameters) {
            var result = new ContentFileResult();
            List<string> files;
            if (!importParameters.Directory.IsNullOrWhiteSpace() && Directory.Exists(importParameters.Directory)) {
                files = Directory.GetFiles(importParameters.Directory, $"*{importParameters.FileType.GetExtension()}").ToList();
            } else {
                files = importParameters.Files;
            }
            if (files.IsEmpty()) {
                return result;
            }

            foreach (var file in files) {
                var contentFile = ReadFromPath(importParameters, file);
                result.Exports.Add(new ContentFileData {
                    File = contentFile,
                    FullPath = file
                });
            }

            return result;
        }

        private static ContentFile ReadFromPath(ContentFileImportParameters importParameters, string file) {
            switch (importParameters.FileType) {
                case FileExportType.Text:
                case FileExportType.CommaSeparatedValues:
                    return ReadAsSeparatedValueFile(importParameters, file);
                case FileExportType.Excel:
                    return ReadAsExcelFile(importParameters, file);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static ContentFile ReadAsSeparatedValueFile(ContentFileImportParameters importParameters, string file) {
            var contentFile = new ContentFile {
                FileName = file,
            };
            List<List<string>> lines;
            if (importParameters.EscapeQuotedValues) {
                lines = ParseAdvancedSeparatedValues(importParameters, file);
            } else {
                lines = ParseSimpleSeparatedValues(importParameters, file);
            }
            var didReadHeader = false;
            foreach (var lineItems in lines) {
                if (importParameters.IncludeHeaders && !didReadHeader) {
                    foreach (var lineItem in lineItems) {
                        contentFile.AddHeader(lineItem);
                    }
                    didReadHeader = true;
                    continue;
                }
                var contentLine = contentFile.AddLine(importParameters.IncludeHeaders ? contentFile.Headers.Count : new int?());
                foreach (var lineItem in lineItems) {
                    contentLine.AddValue(lineItem);
                }
            }
            return contentFile;
        }

        private static List<List<string>> ParseSimpleSeparatedValues(ContentFileImportParameters importParameters, string file) {
            using (var sr = new StreamReader(file)) {
                var lines = new List<List<string>>();
                while (!sr.EndOfStream) {
                    var line = sr.ReadLine();
                    if (line == null) { continue; }
                    if (line.Contains("sep=", StringComparison.InvariantCultureIgnoreCase)) {
                        importParameters.Separator = line.Split('=')[1].To<char>();
                        continue;
                    }
                    lines.Add(line.Split(importParameters.Separator).ToList());
                }
                return lines;
            }
        }

        private static List<List<string>> ParseAdvancedSeparatedValues(ContentFileImportParameters importParameters, string file) {
            using (var sr = new StreamReader(file, importParameters.Encoding)) {
                var lines = new List<List<string>>();
                var insideQuotation = false;
                var currentEntry = new StringBuilder();
                var first = true;
                while (!sr.EndOfStream) {
                    var line = sr.ReadLine();
                    if (line == null) { continue; }
                    if (first && line.StartsWith("sep=", StringComparison.InvariantCultureIgnoreCase)) {
                        importParameters.Separator = line.Split('=')[1].To<char>();
                        first = false;
                        continue;
                    }
                    first = false;
                    if (!insideQuotation) {
                        if (sr.EndOfStream && line.IsNullOrWhiteSpace()) { continue; }
                        lines.Add(new List<string>());
                    }
                    for (var index = 0; index < line.Length; index++) {
                        var c = line[index];
                        var next = (index + 1 < line.Length) ? line[index + 1] : new char?();
                        if (insideQuotation) {
                            if (c == '"') {
                                if ((next == null || next != '"')) {
                                    insideQuotation = false;
                                } else {
                                    index++;
                                    currentEntry.Append('"');
                                }
                            } else {
                                currentEntry.Append(c);
                            }
                        } else if (c == importParameters.Separator) {
                            lines[lines.Count - 1].Add(GetEntry(currentEntry, importParameters.TrimValues));
                            currentEntry.Clear();
                        } else if (c == '"' && (next == null || next != '"')) {
                            insideQuotation = true;
                        } else if (c == '"' && (next != null && next == '"')) {
                            var secondNext = (index + 2 < line.Length) ? line[index + 2] : new char?();
                            if (secondNext == null || secondNext == importParameters.Separator) {
                                index++;
                            } else {
                                currentEntry.Append(c);
                            }
                        } else {
                            currentEntry.Append(c);
                        }
                    }
                    if (!insideQuotation) {
                        lines[lines.Count - 1].Add(GetEntry(currentEntry, importParameters.TrimValues));
                        currentEntry.Clear();
                    } else {
                        currentEntry.Append('\n');
                    }
                }
                return lines;
            }
        }

        private static string GetEntry(StringBuilder entry, bool trim) {
            if (trim) {
                return entry.Trim().ToString();
            }
            return entry.ToString();
        }

        private static ContentFile ReadAsExcelFile(ContentFileImportParameters importParameters, string file) {
            var contentFile = new ContentFile {
                FileName = file,
            };
            using (var excel = new ExcelPackage(new FileInfo(file))) {
                var worksheet = excel.Workbook.Worksheets.FirstOrDefault();
                if (importParameters.Worksheet.HasValue()) {
                    worksheet = excel.Workbook.Worksheets.FirstOrDefault(w => w.Name.EqualsIgnoreCase(importParameters.Worksheet));
                }

                if (worksheet == null) {
                    return contentFile;
                }

                for (var rowIndex = 1; rowIndex <= worksheet.Dimension.Rows; rowIndex++) {
                    if (importParameters.IncludeHeaders && rowIndex == 1) {
                        for (var colIndex = 1; colIndex <= worksheet.Dimension.Columns; colIndex++) {
                            var cellValue = worksheet.Cells[rowIndex, colIndex].Value;
                            contentFile.AddHeader(new ContentHeader {
                                Name = cellValue.To<string>()
                            });
                        }
                    } else {
                        var contentLine = contentFile.AddLine();
                        for (var colIndex = 1; colIndex <= worksheet.Dimension.Columns; colIndex++) {
                            var cellValue = worksheet.Cells[rowIndex, colIndex].Value;
                            contentLine.AddValue(cellValue);
                        }
                    }
                }
            }
            return contentFile;
        }
    }
}