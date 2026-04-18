using BECOSOFT.Utilities.Barcode.Interfaces;
using BECOSOFT.Utilities.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace BECOSOFT.Utilities.Barcode {
    /// <summary>
    /// Class to convert and create barcodes
    /// </summary>
    public static class BarcodeCoder {
        /// <summary>
        /// Convert a numeric string value to an encoded barcode according to the specified type.
        /// </summary>
        /// <param name="type">BarcodeType to convert to</param>
        /// <param name="value">String value to convert</param>
        /// <exception cref="ArgumentOutOfRangeException">When BarcodeType is unknown.</exception>
        /// <exception cref="ArgumentException">The supplied string is not a valid value for the specified <see cref="type"/></exception>
        /// <returns></returns>
        public static string ConvertTo(BarcodeType type, string value) {
            var barcode = GetBarcode(type);
            return barcode.Encode(value);
        }

        /// <summary>
        /// Validates a barcode by the specified BarcodeType
        /// </summary>
        /// <param name="type">BarcodeType to convert to</param>
        /// <param name="barcode">The barcode to validate</param>
        /// <exception cref="ArgumentOutOfRangeException">When BarcodeType is unknown.</exception>
        /// <returns>A boolean result</returns>
        public static bool IsValid(BarcodeType type, string barcode) {
            var barcoder = GetBarcode(type);
            return barcoder.IsValid(barcode);
        }

        /// <summary>
        /// Validates a barcode by the specified BarcodeTypes
        /// </summary>
        /// <param name="types">BarcodeTypes to convert to</param>
        /// <param name="barcode">The barcode to validate</param>
        /// <exception cref="ArgumentOutOfRangeException">When BarcodeType is unknown.</exception>
        /// <returns>A boolean result</returns>
        public static bool IsValid(List<BarcodeType> types, string barcode) {
            foreach (var type in types) {
                var barcoder = GetBarcode(type);
                if (barcoder.IsValid(barcode)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Cleans a barcode string 
        /// </summary>
        /// <param name="value">Value to clean</param>
        /// <returns>Cleaned barcode value</returns>
        public static string Clean(string value) {
            if (string.IsNullOrWhiteSpace(value)) {
                return value;
            }
            var builder = new StringBuilder(value);
            builder.Replace("&", "1");
            builder.Replace("é", "2");
            builder.Replace("\"", "3");
            builder.Replace("'", "4");
            builder.Replace("(", "5");
            builder.Replace("§", "6");
            builder.Replace("è", "7");
            builder.Replace("!", "8");
            builder.Replace("ç", "9");
            builder.Replace("à", "0");
            builder.Replace("<", "");
            builder.Replace(">", "");
            builder.Replace("/", ".");
            builder.Replace(":", "/");
            //voor pelckmans;
            builder.Replace("°", "-");
            builder.Replace("£", "|");
            builder.Replace("µ", "|");
            builder.Replace("ù", "|");
            builder.Replace("~", "|");
            builder.Replace(")", "-");
            return builder.ToString();
        }

        private static IBarcode GetBarcode(BarcodeType type) {
            switch (type) {
                case BarcodeType.Ean13:
                    return new Ean13();
                case BarcodeType.Ean8:
                    return new Ean8();
                case BarcodeType.Code11:
                    return new Code11();
                case BarcodeType.Code128:
                    return new Code128();
                case BarcodeType.Code128Auto:
                    return new Code128Auto();
                case BarcodeType.Code128C:
                    return new Code128C();
                case BarcodeType.Gtin:
                    return new Gtin();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// Creates a full Ean13 from a supplied string value
        /// </summary>
        /// <param name="value">A string value of 12 digit characters length</param>
        /// <exception cref="ArgumentException">The supplied string is not a valid numeric string value</exception>
        /// <returns>The supplied barcode value with the checksum added.</returns>
        public static string CreateEan13(string value) {
            if (string.IsNullOrWhiteSpace(value) || value.Length != Ean13.LengthWithoutCheck) {
                throw new ArgumentException();
            }
            var ean13Barcode = new Ean13();
            var checksumCalculation = ean13Barcode.GetChecksum(value);
            if (!checksumCalculation.IsValid) {
                throw new ArgumentException();
            }
            return value + checksumCalculation.CheckDigit;
        }

        /// <summary>
        /// Returns the length of the barcode.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="countCheckDigit"></param>
        /// <returns></returns>
        public static int GetLength(BarcodeType type, bool countCheckDigit = true) {
            IBarcodeNumberSeriesEnabled barcodeNumberSeriesEnabled;
            switch (type) {
                case BarcodeType.Ean13:
                    barcodeNumberSeriesEnabled = new Ean13();
                    break;
                case BarcodeType.Ean8:
                    barcodeNumberSeriesEnabled= new Ean8();
                    break;
                case BarcodeType.Gtin:
                    barcodeNumberSeriesEnabled = new Gtin();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            if (!countCheckDigit) {
                return barcodeNumberSeriesEnabled.GetLengthWithoutCheckDigit();
            }
            return barcodeNumberSeriesEnabled.GetLength();
        }

        /// <summary>
        /// Returns the check digit for the specified <paramref name="type"/> and <paramref name="value"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetChecksum(BarcodeType type, string value) {
            IChecksummable barcodeNumberSeriesEnabled;
            switch (type) {
                case BarcodeType.Ean13:
                    barcodeNumberSeriesEnabled = new Ean13();
                    break;
                case BarcodeType.Ean8:
                    barcodeNumberSeriesEnabled= new Ean8();
                    break;
                case BarcodeType.Gtin:
                    barcodeNumberSeriesEnabled = new Gtin();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            var checksum = barcodeNumberSeriesEnabled.GetChecksum(value);
            if (!checksum.IsValid) {
                throw new ArgumentException("Invalid barcode provided", nameof(value));
            }
            return checksum.CheckDigit.To<int>();
        }
    }
}