using BECOSOFT.Utilities.Barcode.Interfaces;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Barcode {
    public class Code128C : Barcode, IBarcode {
        public override BarcodeType Type => BarcodeType.Code128C;

        public override bool IsValid(string value) {
            if (value.IsNullOrWhiteSpace()) {
                return false;
            }
            var valueCharArray = value.ToCharArray();
            if (valueCharArray.Any(c => !char.IsDigit(c))) {
                return false;
            }
            return true;
        }

        protected override string PerformEncode(string value) {
            var result = new List<char>();

            var temp = 105;
            var multiplicator = 0;

            result.Add((char) 210);
            for (var i = 0; i < value.Length; i += 2) {
                var part = value.Substring(i, 2).To<int>();
                multiplicator += 1;
                var dummy = part + (part < 95 ? 32 : 105);
                temp += part * multiplicator;
                result.Add((char) dummy);
                System.Diagnostics.Debug.WriteLine("{0} : {1}", dummy, (char)dummy);
            }
            var mod = temp % 103;
            var checkDigit = mod + (mod < 95 ? 32 : 105);
            result.Add((char)checkDigit);
            result.Add((char) 211);

            var barcode = new string(result.ToArray());
            return barcode;
        }
    }
}