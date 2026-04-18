using System.Diagnostics;

namespace BECOSOFT.Utilities.IO {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class ContentHeader {
        public string Name { get; set; } = "";
        public bool Enclose { get; set; }

        public override string ToString() {
            var escapedDoubleQuote = false;
            var value = Name;
            if (value.Contains("\"")) {
                escapedDoubleQuote = true;
                value = value.Replace("\"", "\"\"");
            }

            if (Enclose || escapedDoubleQuote) {
                return $"\"{value}\"";
            }
            return value;
        }

        public static ContentHeader Create(string header) {
            return new ContentHeader {Name = header };
        }

        public ContentHeader Copy() {
            return Create(Name);
        }

        private string DebuggerDisplay => $"Header: '{Name}'";
    }
}