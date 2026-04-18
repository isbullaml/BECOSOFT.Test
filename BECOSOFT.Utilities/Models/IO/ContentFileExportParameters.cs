using BECOSOFT.Utilities.IO;
using System.Collections.Generic;
using System.Text;

namespace BECOSOFT.Utilities.Models.IO {
    public class ContentFileExportParameters {
        /// <summary>
        /// A collection of <see cref="ContentFile"/> objects to Export.
        /// </summary>
        public List<ContentFile> Files { get; set; }

        /// <summary>
        /// Defines the <see cref="Separator"/> character.
        /// </summary>
        public string Separator { get; set; } = ";";

        /// <summary>
        /// Includes a header before the column headers (to define a file version for example). Only applies to <see cref="F:FileExportType.Csv"/>.
        /// </summary>
        public string PreHeader { get; set; }

        /// <summary>
        /// Defines if the headers in the <see cref="Files"/> need to be included.
        /// </summary>
        public bool IncludeHeaders { get; set; } = true;

        /// <summary>
        /// Defines the file type of the result files.
        /// </summary>
        public FileExportType FileType { get; set; } = FileExportType.CommaSeparatedValues;

        /// <summary>
        /// Defines the file extension of the result files. If not defined, the extension is based on the <see cref="FileType"/>.
        /// </summary>
        public string FileExtension { get; set; }

        /// <summary>
        /// Defines the directory to save the files to.
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// Encoding for file export. Default: UTF8 without BOM
        /// </summary>
        public Encoding Encoding { get; set; } = new UTF8Encoding(false, true);

        /// <summary>
        /// Defines if the columns need to be fitted to the content.
        /// </summary>
        public bool AutoFitColumns { get; set; } = false;
    }
}