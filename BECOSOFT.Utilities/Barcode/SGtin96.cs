using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Barcode {
    public sealed class SGtin96 {
        private readonly int _lengthBits = 96;
        private readonly int _lengthHex = 24;
        private readonly HashSet<int> _validHexLengths = new HashSet<int> { 24, 32 };
        private HashSet<int> _validGtinLengths = new HashSet<int> { 13, 14 };
        private readonly int _gtinLength = Gtin.Length;
        private readonly string _header = "00110000";
        private DateTime _baseDate = new DateTime(2024, 1, 1);

        private readonly Dictionary<int, Partition> _mappings = new Dictionary<int, Partition> {
            { Partition.Six.IndexValue, Partition.Six },
            { Partition.Seven.IndexValue, Partition.Seven },
            { Partition.Eight.IndexValue, Partition.Eight },
            { Partition.Nine.IndexValue, Partition.Nine },
            { Partition.Ten.IndexValue, Partition.Ten },
            { Partition.Eleven.IndexValue, Partition.Eleven },
            { Partition.Twelve.IndexValue, Partition.Twelve },
        };

        public SGtin96Components Parse(string input) {
            if (!_validHexLengths.Contains(input.Length)) {
                return new SGtin96Components(input);
            }
            var data = input.ToLower().PadLeft(_lengthHex, '0');
            var bytes = string.Join("", data.Select(d => Convert.ToString(Convert.ToInt32(d.ToString(), 16), 2).PadLeft(4, '0')));
            var fill = _lengthBits - bytes.Length > 0 ? new string('0', bytes.Length) : "";
            var binary = fill + bytes;
            var header = binary.Substring(0, 8);
            if (!header.Equals(_header)) {
                return new SGtin96Components(input);
            }
            var filterValue = Convert.ToInt32(binary.Substring(8, 3), 2);
            var partitionRange = binary.Substring(11, 3);
            var partitionValue = Convert.ToInt32(partitionRange, 2);
            if (!_mappings.TryGetValue(partitionValue, out var partition)) {
                return new SGtin96Components(input);
            }
            var (companyPrefixBits, itemReferenceIndicatorBits) = partition.Definition;
            var companyPrefixLength = partition.CompanyPrefixLength;
            if (companyPrefixLength >= Math.Pow(10, companyPrefixLength).To<int>()) {
                return new SGtin96Components(input);
            }
            var companyPrefixValue = Convert.ToInt64(binary.Substring(14, companyPrefixBits), 2).ToString().PadLeft(companyPrefixLength, '0');
            var itemReferenceLength = 13 - companyPrefixLength;
            var offset = 14 + companyPrefixBits;
            var itemReferenceValue = Convert.ToInt64(binary.Substring(offset, 58 - offset), 2).ToString().PadLeft(itemReferenceLength, '0');
            var ean = $"{itemReferenceValue.Substring(0, 1)}{companyPrefixValue}{itemReferenceValue.Substring(1)}";
            var gtinCoder = new Gtin();
            var checkDigit = gtinCoder.GetChecksum(ean);
            var partialGtin = $"{ean}{checkDigit.CheckDigit}";
            var gtin = partialGtin.PadLeft(_gtinLength, '0');

            var serialNumberBytes = binary.Substring(58);
            var serialNumber = Convert.ToInt64(serialNumberBytes, 2).ToString();
            var isSample = serialNumber.Length == 11 && serialNumber.StartsWith("9");
            if (isSample) {
                var timePart = serialNumber.Substring(1).To<double>() / 100.0;
                var date = _baseDate.Add(TimeSpan.FromMilliseconds(timePart));
                isSample = date.LiesBetween(_baseDate, DateTime.Now);
            }
            var parsed = "urn:epc:tag:sgtin-96:{0}.{1}.{2}.{3}".FormatWith(filterValue, companyPrefixValue, itemReferenceValue, serialNumber);
            var bitMask = new string('1', 58) + new string('0', 58 - (Math.Round(58.0 / 4.0).To<int>() * 4));
            var maskPart = string.Join("", bitMask.Partition(4).Select(b => Convert.ToByte(string.Join("", b), 2).ToString("X")));
            var mask = maskPart.PadRight(_lengthHex, '0');
            return new SGtin96Components(gtin, parsed, input, isSample, mask);
        }

        private class Partition {
            private readonly int _rawValue;
            public int IndexValue => 6 - Value;
            public int Value => _rawValue;
            public int CompanyPrefixLength => _rawValue + 6;

            public (int companyPrefixBits, int itemReferenceIndicatorBits) Definition {
                get {
                    switch (_rawValue + 6) {
                        case 6:
                            return (20, 24);
                        case 7:
                            return (24, 20);
                        case 8:
                            return (27, 17);
                        case 9:
                            return (30, 14);
                        case 10:
                            return (34, 10);
                        case 11:
                            return (37, 7);
                        case 12:
                            return (40, 4);
                    }
                    return (0, 0);
                }
            }

            private Partition(int value) {
                _rawValue = value;
            }

            public static Partition Six => new Partition(0);
            public static Partition Seven => new Partition(1);
            public static Partition Eight => new Partition(2);
            public static Partition Nine => new Partition(3);
            public static Partition Ten => new Partition(4);
            public static Partition Eleven => new Partition(5);
            public static Partition Twelve => new Partition(6);
        }
    }

    public class SGtin96Components {
        public string Gtin { get; set; }
        public string Sgtin96 { get; set; }
        public string Encoded { get; set; }
        public bool IsSample { get; set; }
        public bool IsValid { get; set; }

        public string Mask { get; set; }

        public SGtin96Components(string gtin, string sgtin96, string encoded, bool isSample, string mask) {
            Gtin = gtin;
            Sgtin96 = sgtin96;
            Encoded = encoded;
            IsSample = isSample;
            Mask = mask;
            IsValid = true;
        }

        public SGtin96Components(string encoded) {
            Encoded = encoded;
            IsValid = false;
        }
    }
}