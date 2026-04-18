using BECOSOFT.Utilities.IO;
using System.Collections.Generic;
using System.Text;

namespace BECOSOFT.Utilities.Models.IO {
    public class ContentFileImportParameters {
        /// <summary>
        /// A collection of file paths that will be imported.
        /// </summary>
        public List<string> Files { get; set; } = new List<string>();
        /// <summary>
        /// Defines the directory to read the files (based on <see cref="FileType"/>) from.
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// Defines the <see cref="Separator"/> character.
        /// </summary>
        public char Separator { get; set; } = ';';

        /// <summary>
        /// Defines if there are headers present.
        /// </summary>
        public bool IncludeHeaders { get; set; } = true;

        /// <summary>
        /// Defines the file type of the result files.
        /// </summary>   
        public FileExportType FileType { get; set; } = FileExportType.CommaSeparatedValues;
        
        /// <summary>
        /// Defines if the import service needs to escape the following example: "test",12,"test" to the following values: test, 12 and test.
        /// </summary>
        public bool EscapeQuotedValues { get; set; } = true;

        /// <summary>
        /// Defines if the parsed values need to be trimmed.
        /// </summary>
        public bool TrimValues { get; set; } = false;

        /// <summary>
        /// Defines the worksheet to read when using <see cref="FileType"/> is <see cref="FileExportType.Excel"/>.
        /// </summary>
        public string Worksheet { get; set; }

        public Encoding Encoding { get; set; } = Encoding.UTF8;
    }
}