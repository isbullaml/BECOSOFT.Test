using BECOSOFT.Utilities.Extensions.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BECOSOFT.Utilities.IO {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class ContentFile {
        public List<ContentHeader> Headers { get; set; } = new List<ContentHeader>();
        public List<ContentLine> Lines { get; set; } = new List<ContentLine>();
        public string FileName { get; set; }

        public ContentHeader AddHeader(string header) {
            var contentHeader = ContentHeader.Create(header);
            Headers.Add(contentHeader);
            return contentHeader;
        }

        public ContentHeader AddHeader(ContentHeader header) {
            Headers.Add(header);
            return header;
        }

        /// <summary>
        /// Constructs a new <see cref="ContentLine"/> with the optional <paramref name="capacity"/> specified, adds it to the <see cref="Lines"/> and returns the constructed <see cref="ContentLine"/>.
        /// </summary>
        /// <param name="capacity"></param>
        /// <returns></returns>
        public ContentLine AddLine(int? capacity = null) {
            return AddLine(new ContentLine(capacity));
        }

        /// <summary>
        /// Add a <paramref name="line"/> to the <see cref="Lines"/> and returns <paramref name="line"/>.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public ContentLine AddLine(ContentLine line) {
            Lines.Add(line);
            return line;
        }

        /// <summary>
        /// Creates a new <see cref="ContentFile"/> with the content of the current and <see cref="other"/>.
        /// The <see cref="Headers"/> of the current file (if available) are used, otherwise the <see cref="Headers"/> of <see cref="other"/> are used.
        /// The caller needs to handle <see cref="Lines"/> with a different number of values
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public ContentFile Merge(ContentFile other) {
            var contentFile = new ContentFile();
            contentFile.Append(this);
            contentFile.Append(other);
            return contentFile;
        }

        /// <summary>
        /// Append the content of <see cref="other"/> to the the current <see cref="ContentFile"/> .
        /// The <see cref="Headers"/> of the current file (if available) are used, otherwise the <see cref="Headers"/> of <see cref="other"/> are used.
        /// The caller needs to handle <see cref="Lines"/> with a different number of values.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public void Append(ContentFile other) {
            if (Headers.IsEmpty()) {
                Headers.AddRange(other.Headers.Select(h => h.Copy()));
            }
            Lines.AddRange(other.Lines.Select(l => l.Copy()));
        }

        private string DebuggerDisplay => $"File: '{FileName}', {Headers.Count} headers, {Lines.Count} lines";
    }
}