using BECOSOFT.Utilities.Barcode.Interfaces;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Models;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Barcode {
    public class Gtin : Barcode, IBarcode, IChecksummable, IBarcodeNumberSeriesEnabled {
        public override BarcodeType Type => BarcodeType.Gtin;

        /// <summary>
        /// The length of an GTIN-barcode
        /// </summary>
        public static readonly int Length = 14;

        /// <summary>
        /// The length of an GTIN-barcode without the check digit
        /// </summary>
        public static readonly int LengthWithoutCheck = Length - 1;

        public override bool IsValid(string value) {
            if (value.IsNullOrWhiteSpace() || value.Length != Length) {
                return false;
            }
            if (value.Length == Ean13.Length) {
                return (new Ean13()).IsValid(value);
            }
            var valueCharArray = value.ToCharArray();
            if (valueCharArray.Any(c => !char.IsDigit(c))) {
                return false;
            }
            var checksumCalculation = GetChecksum(value);
            if (!checksumCalculation.IsValid) {
                return false;
            }
            // compare with checkdigit
            return checksumCalculation.CheckDigit == valueCharArray[LengthWithoutCheck].ToString();
        }

        protected override string PerformEncode(string value) {
            value = value.Substring(1);
            var firstChar = value[0].ToString().To<int>().ToString()[0];
            var barcode = new List<char> {
                firstChar,
                (char)(65 + value[1].ToString().To<int>())
            };
            var first = value[0].ToString().To<int>();
            for (var i = 2; i <= 6; i++) {
                var tableA = false;
                if (i == 2) {
                    tableA = new Range<int>(0, 3).Contains(first);
                } else if (i == 3) {
                    tableA = new[] { 0, 4, 7, 8 }.Contains(first);
                } else if (i == 4) {
                    tableA = new[] { 0, 1, 4, 5, 9 }.Contains(first);
                } else if (i == 5) {
                    tableA = new[] { 0, 2, 5, 6, 7 }.Contains(first);
                } else if (i == 6) {
                    tableA = new[] { 0, 3, 6, 8, 9 }.Contains(first);
                }
                if (tableA) {
                    barcode.Add((char)(65 + value[i].ToString().To<int>()));
                } else {
                    barcode.Add((char)(75 + value[i].ToString().To<int>()));
                }
            }
            barcode.Add('*');
            for (var i = 7; i <= 12; i++) {
                barcode.Add((char)(97 + value[i].ToString().To<int>()));
            }
            barcode.Add('+');
            return new string(barcode.ToArray());
        }

        public Checksum GetChecksum(string value) {
            var valueCharArray = value.ToCharArray();
            var checksum = 0;
            for (var index = 0; index < LengthWithoutCheck; index++) {
                var c = valueCharArray[index];
                if (!char.IsDigit(c)) {
                    return new Checksum { IsValid = false };
                }
                var digit = c.ToString().To<int>();
                checksum += index % 2 == 0 ? digit * 3 : digit ;
            }
            var checksumRemainder = 10 - (checksum % 10);
            var checkDigit = checksumRemainder % 10;
            return new Checksum {
                IsValid = true,
                CheckDigit = checkDigit.ToString(),
            };
        }

        public int GetLength() => Length;

        public int GetLengthWithoutCheckDigit() => LengthWithoutCheck;
    }
}