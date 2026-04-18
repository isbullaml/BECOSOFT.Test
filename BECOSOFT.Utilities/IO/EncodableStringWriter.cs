using System.IO;
using System.Text;

namespace BECOSOFT.Utilities.IO {
    /// <summary>
    /// A <see cref="StringWriter"/> with an <see cref="Encoding"/>
    /// </summary>
    public class EncodableStringWriter: StringWriter {
        /// <summary>
        /// The encoding of the string
        /// </summary>
        public override Encoding Encoding { get; }

        public EncodableStringWriter(Encoding encoding) {
            Encoding = encoding;
        }

        /// <summary>
        /// Gets an <see cref="EncodableStringWriter"/> in <see cref="UTF8Encoding"/>
        /// </summary>
        public static EncodableStringWriter Utf8 => new EncodableStringWriter(Encoding.UTF8);
        /// <summary>
        /// Gets an <see cref="EncodableStringWriter"/> in Windows-1252 encoding
        /// </summary>

        public static EncodableStringWriter Windows1252 => new EncodableStringWriter(Encoding.GetEncoding("windows-1252"));
    }
}
