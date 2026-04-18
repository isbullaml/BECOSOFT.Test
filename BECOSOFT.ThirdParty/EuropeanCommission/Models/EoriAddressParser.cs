using BECOSOFT.Utilities.Converters;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.ThirdParty.EuropeanCommission.Models {
    public static class EoriAddressParser {
        public static EoriAddress Parse(string country, string addressLine, string postalCode, string city) {

            var response = new EoriAddress(country, postalCode, city);

            ParseNumericParts(response, addressLine.Split(' '));

            return response;
        }



        private static void ParseNumericParts(EoriAddress response, IReadOnlyList<string> splitAddress) {
            var numericParts = GetNumericParts(splitAddress);
            if (numericParts.Count >= 2) {
                var numberIndex = numericParts[numericParts.Count - 1];
                response.Number = splitAddress[numberIndex];
                response.Street = string.Join(" ", splitAddress.Take(numberIndex));
            } else if (numericParts.Count > 0) {
                var index = numericParts[0];
                if (index == 0) {
                    response.Street = string.Join(" ", splitAddress.Skip(1));
                } else {
                    response.Street = string.Join(" ", splitAddress.Take(index));
                }

                response.Number = splitAddress[index];
                if (index != 0 && splitAddress.Count > index) {
                    response.Box = string.Join(" ", splitAddress.Skip(index + 1).Take(splitAddress.Count - (index + 1)));
                }
            } else {
                response.Street = string.Join(" ", splitAddress);
            }
        }

        private static List<int> GetNumericParts(IEnumerable<string> splitAddress) {
            return splitAddress.Select((part, i) => new {
                Part = part.To<int>(),
                Index = i
            }).Where(item => item.Part != 0).Select(item => item.Index).ToList();
        }
    }
}