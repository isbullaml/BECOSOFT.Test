using BECOSOFT.Utilities.Barcode.Interfaces;
using BECOSOFT.Utilities.Collections;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Models;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Barcode {
    /* Source: 
    http://grandzebu.net/informatique/codbar-en/code128.htm
    http://grandzebu.net/informatique/codbar/code128_C%23.txt
    https://en.wikipedia.org/wiki/Code_128
     */
    public class Code128Auto : Barcode, IBarcode {
        public override BarcodeType Type => BarcodeType.Code128Auto;

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
            var chop = Chop(valueCharArray);
            for (var i = 0; i < chop.Count; i++) {
                var part = chop[i];
                if (part.Key) {
                    result.Add((char) (i == 0 ? 210 : 204));
                    for (var n = 0; n < part.Value.Count; n += 2) {
                        var temp = "" + part.Value[n] + part.Value[n + 1];
                        var intValue = int.Parse(temp);
                        var dummy = intValue + (intValue < 95 ? 32 : 105);
                        result.Add((char) dummy);
                    }
                } else {
                    result.Add((char) (i == 0 ? 209 : 205));
                    result.AddRange(part.Value);
                }
            }
            var checksum = 0;
            for (var index = 0; index < result.Count; index++) {
                var dummy = (int) result[index];
                dummy = dummy - (dummy < 127 ? 32 : 105);
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

        private KeyValueList<bool, List<char>> Chop(char[] value) {
            var result = new KeyValueList<bool, List<char>>();
            
            var currentList = new List<char>();
            var isDigit = false;
            foreach (var c in value) {
                var tempIsDigit = char.IsDigit(c);
                if (isDigit != tempIsDigit) {
                    if (currentList.Count == 0) {
                        currentList.Add(c);
                    } else {
                        result.Add(isDigit, currentList);
                        currentList = new List<char> { c };
                    }
                } else {
                    currentList.Add(c);
                }
                isDigit = tempIsDigit;
            }
            result.Add(isDigit, currentList);
            result = result.Where(l => l.Value.Count != 0).ToList();

            var temp = new KeyValueList<bool, List<char>>();
            for (var i = 0; i < result.Count; i++) {
                var r = result[i];
                if (i + 1 >= result.Count) {
                    temp.Add(r);
                    continue;
                }
                var next = result[i + 1];
                if (next.Key == r.Key) {
                    r.Value.AddRange(next.Value);
                    i++;
                }
                temp.Add(r);
            }
            result = temp;
            if (result.Count > 1) {
                for (var i = 0; i < result.Count; i++) {
                    var r = result[i];
                    if (!r.Key) { continue; }
                    var isEven = r.Value.Count % 2 == 0;
                    if (isEven) { continue; }
                    if (i > 0) {
                        result[i - 1].Value.Add(r.Value[0]);
                        r.Value.RemoveAt(0);
                    } else if (i + 1 < result.Count) {
                        var lastIndex = r.Value.Count - 1;
                        result[i + 1].Value.Add(r.Value[lastIndex]);
                        r.Value.RemoveAt(lastIndex);
                    }
                }
            }
            if (result.Count > 1) {
                for (var i = 0; i < result.Count; i++) {
                    var r = result[i];
                    if (i + 1 >= result.Count) { continue; }
                    var next = result[i + 1];
                    var merge = false;
                    if(r.Key == next.Key) {
                        merge = true;
                    } else if (r.Key) {
                        if (i == 0) {
                            if (r.Value.Count < 4) {
                                merge = true;
                            }
                        } else {
                            if (r.Value.Count < 6) {
                                merge = true;
                            }
                        }
                    } else if (next.Key) {
                        if (i == result.Count - 1) {
                            if (next.Value.Count < 4) {
                                merge = true;
                            }
                        } else {
                            if (next.Value.Count < 6) {
                                merge = true;
                            }
                        }
                    }
                    if (merge) {
                        r.Value.AddRange(next.Value);
                        result.RemoveAt(i + 1);
                        if (r.Key != next.Key) {
                            result[i] = KeyValuePair.Create(false, r.Value);
                        }
                        i = 0;
                    }
                }
            }

            return result;
        }
    }
}