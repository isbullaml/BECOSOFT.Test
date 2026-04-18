using BECOSOFT.Utilities.Annotations;
using System.IO;
using System.Threading.Tasks;

namespace BECOSOFT.Utilities.Extensions.IO {
    public static class StreamWriterExtensions {
        [StringFormatMethod("format")]
        public static async Task WriteAsync(this StreamWriter writer, string format, params object[] args) {
            var value = format.FormatWith(args);
            await writer.WriteAsync(value);
        }

        [StringFormatMethod("format")]
        public static async Task WriteLineAsync(this StreamWriter writer, string format, params object[] args) {
            var value = format.FormatWith(args);
            await writer.WriteLineAsync(value);
        }

        [StringFormatMethod("format")]
        public static void Write(this StreamWriter writer, string format, params object[] args) {
            var value = format.FormatWith(args);
            writer.Write(value);
        }

        [StringFormatMethod("format")]
        public static void WriteLine(this StreamWriter writer, string format, params object[] args) {
            var value = format.FormatWith(args);
            writer.WriteLine(value);
        }
    }
}