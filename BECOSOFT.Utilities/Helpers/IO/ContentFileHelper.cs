using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.IO;
using System.IO;

namespace BECOSOFT.Utilities.Helpers.IO {
    public static class ContentFileHelper {
        public static void WriteTo(this ContentFile file, TextWriter writer, ContentFileWriteOptions options) {
            if (options.PreHeader.HasValue()) {
                writer.WriteLine(options.PreHeader);
            }
            if (options.IncludeHeaders && file.Headers.HasAny()) {
                writer.WriteLine(string.Join(options.Separator, file.Headers));
            }
            for (var index = 0; index < file.Lines.Count; index++) {
                var line = file.Lines[index];
                var lineToWrite = string.Join(options.Separator, line.Values);
                if (index < file.Lines.Count - 1) {
                    writer.WriteLine(lineToWrite);
                } else {
                    writer.Write(lineToWrite);
                }
            }
        }
    }
}
