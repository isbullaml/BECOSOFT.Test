using System;
using System.Diagnostics;
using System.Globalization;

namespace BECOSOFT.Utilities.IO {
     [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class ContentValue {
        public object Value { get; set; }
        public bool Enclose { get; set; }

        public ContentValue(bool enclose) {
            Enclose = enclose;
        }

        public ContentValue() {
        }

        public override string ToString() {
            var escapedDoubleQuote = false;
            var value = GetStringValue();
            if (value.Contains("\"")) {
                escapedDoubleQuote = true;
                value = value.Replace("\"", "\"\"");
            }

            if (Enclose || escapedDoubleQuote) {
                return $"\"{value}\"";
            }
            return value;
        }

        public string GetStringValue() {
            if (Value == null) { return string.Empty; }
            var strValue = Value as string;
            if (strValue != null) {
                return strValue;
            }
            var convertible = Value as IConvertible;
            if (convertible != null) {
                return convertible.ToString(CultureInfo.InvariantCulture);
            }
            var formattable = Value as IFormattable;
            if (formattable != null) {
                return formattable.ToString(null, CultureInfo.InvariantCulture);
            }
            return Value.ToString();
        }

        public ContentValue Copy() {
            return new ContentValue {
                Value = Value,
                Enclose = Enclose,
            };
        }

        private string DebuggerDisplay => $"Value: '{GetStringValue()}', Enclosed? {Enclose}";
    }
}