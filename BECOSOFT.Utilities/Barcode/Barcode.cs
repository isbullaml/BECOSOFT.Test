using System;

namespace BECOSOFT.Utilities.Barcode {
    public abstract class Barcode {
        public abstract bool IsValid(string value);

        public abstract BarcodeType Type { get; }

        public string Encode(string value) {
            if (!IsValid(value)) {
                throw new ArgumentException($"Invalid value for barcode type {Type}", nameof(value));
            }
            return PerformEncode(value);
        }

        protected abstract string PerformEncode(string value);
    }
}
