using BECOSOFT.Utilities.IO;
using System.Collections.Generic;

namespace BECOSOFT.Utilities.Models.IO {
    public class ContentFileResult {
        public List<ContentFileData> Exports { get; set; }

        public ContentFileResult() {
            Exports = new List<ContentFileData>();
        }
    }
}