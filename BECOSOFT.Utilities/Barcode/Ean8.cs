using BECOSOFT.Utilities.Barcode.Interfaces;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Barcode {
    public class Ean8 : Barcode, IBarcode, IChecksummable, IBarcodeNumberSeriesEnabled {
        public override BarcodeType Type => BarcodeType.Ean8;

        /// <summary>
        /// The length of an EAN8-barcode
        /// </summary>
        public static readonly int Length = 8;

        /// <summary>
        /// The length of an EAN8-barcode without the check digit
        /// </summary>
        public static readonly int LengthWithoutCheck = Length - 1;

        public override bool IsValid(string value) {
            if (value.IsNullOrWhiteSpace() || value.Length != Length) {
                return false;
            }
            var valueCharArray = value.ToCharArray();
            if (valueCharArray.Any(c => !char.IsDigit(c))) {
                return false;
            }
            // calculate checksum
            var checksumCalculation = GetChecksum(value);
            if (!checksumCalculation.IsValid) {
                return false;
            }

            // compare with checkdigit
            return checksumCalculation.CheckDigit == valueCharArray[7].ToString();
        }

        protected override string PerformEncode(string value) {
            var barcode = new List<char> { ':' };
            for (var i = 0; i < 4; i++) {
                barcode.Add((char) (65 + value[i].ToString().To<int>()));
            }
            barcode.Add('*');
            for (var i = 4; i <= 7; i++) {
                barcode.Add((char) (97 + value[i].ToString().To<int>()));
            }
            barcode.Add('+');
            return new string(barcode.ToArray());
        }

        public Checksum GetChecksum(string value) {
            var valueCharArray = value.ToCharArray();
            var checksum = 0;
            for (var i = LengthWithoutCheck - 1; i >= 0; i--) {
                var digit = valueCharArray[i];
                if (!char.IsDigit(digit)) {
                    return new Checksum { IsValid = false };
                }
                if (i % 2 == 0) {
                    checksum += digit * 3;
                } else {
                    checksum += digit;
                }
            }
            var checksumRemainder = checksum % 10;
            var checkDigit = checksumRemainder == 0 ? 10 : 10 - checksumRemainder;
            return new Checksum {
                IsValid = true,
                CheckDigit = checkDigit.ToString()
            };
        }

        public int GetLength() => Length;

        public int GetLengthWithoutCheckDigit() => LengthWithoutCheck;
    }
}