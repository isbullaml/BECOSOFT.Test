using BECOSOFT.Utilities.Barcode.Interfaces;
using BECOSOFT.Utilities.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Barcode {
    public class Code128 : Barcode, IBarcode {
        public override BarcodeType Type => BarcodeType.Code128;

        public override bool IsValid(string value) {
            if (value.IsNullOrWhiteSpace()) {
                return false;
            }
            var valueCharArray = value.ToCharArray();
            // check for invalid characters:
            if (valueCharArray.Any(nibble => (nibble < 32 || nibble > 126) && nibble != 203)) {
                return false;
            }
            return true;
        }

        protected override string PerformEncode(string value) {
            var result = new List<char>();
            var valueCharArray = value.ToCharArray();
            // calculate checksum:
            result.Add((char) 209);
            result.AddRange(valueCharArray);
            var checksum = 0;
            for (var index = 0; index < result.Count; index++) {
                var dummy = (int) result[index];
                dummy = dummy < 127 ? dummy - 32 : dummy - 105;
                if (index == 0) {
                    checksum = dummy;
                }
                checksum = (checksum + (index * dummy)) % 103;
            }
            checksum += checksum < 95 ? 32 : 105;
            result.Add((char) checksum);
            result.Add((char) 211);
            return new string(result.ToArray());
        }
    }
}