using BECOSOFT.Utilities.Barcode.Interfaces;
using BECOSOFT.Utilities.Converters;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BECOSOFT.Utilities.Barcode {
    public class Code11 : Barcode, IBarcode {
        private static readonly List<string> Codes = new List<string> { "101011", "1101011", "1001011", "1100101", "1011011", "1101101", "1001101", "1010011", "1101001", "110101", "101101", "1011001" };

        public override BarcodeType Type => BarcodeType.Code11;

        public override bool IsValid(string value) {
            if (string.IsNullOrWhiteSpace(value)) {
                return false;
            }
            var valueCharArray = value.ToCharArray();
            if (valueCharArray.Any(c => !char.IsDigit(c) && c != '-')) {
                return false;
            }
            return true;
        }

        protected override string PerformEncode(string value) {
            var weight = 1;
            var checksum = 0;
            var dataToEncode = value;
            for (var index = 0; index < value.Length; index++) {
                if (weight == 10) { weight = 1; }
                var charAtIndex = value[index];
                if (charAtIndex != '-') {
                    checksum += charAtIndex.ToString().To<int>();
                } else {
                    checksum += 10 * weight;
                    weight++;
                }
            }
            var checksumRemainder = checksum % 11;
            dataToEncode += checksumRemainder.ToString();
            if (value.Length >= 10) {
                weight = 1;
                var kChecksum = 0;
                for (var index = dataToEncode.Length - 1; index >= 0; index--) {
                    if (weight == 9) { weight = 1; }
                    var charAtIndex = value[index];
                    if (charAtIndex != '-') {
                        kChecksum += charAtIndex.ToString().To<int>() * weight;
                    } else {
                        kChecksum += 10 * weight;
                    }
                    weight++;
                }
                var kChecksumRemainder = kChecksum % 11;
                dataToEncode += kChecksumRemainder.ToString();
            }
            const string space = "0";
            var resultBuilder = new StringBuilder();
            resultBuilder.Append(Codes[11]).Append(space);
            foreach (var c in dataToEncode) {
                var index = c == '-' ? 10 : c.ToString().To<int>();
                resultBuilder.Append(Codes[index]);
                resultBuilder.Append(space);
            }
            resultBuilder.Append(Codes[11]);
            return resultBuilder.ToString();
        }
    }
}