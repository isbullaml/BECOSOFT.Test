using BECOSOFT.Utilities.Extensions;
using System;
using System.Linq;

namespace BECOSOFT.ThirdParty.EuropeanCommission.Models {
    public static class ViesAddressParser {
        public static ViesAddress Parse(string countryCode, string address) {
            address = address.Replace('\uFFFD', ' ');

            var response = new ViesAddress(countryCode, address);

            var splitCharacters = Environment.NewLine.ToCharArray().ToList();
            if (response.CountryCode.EqualsIgnoreCase("BE")) {
                splitCharacters.Add('\u00FF');
            }
            var addressLines = address
                    .Split(new[] { "\n"}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToArray();

            if (addressLines.Length < 2)
            {
                return response;
            }

            var streetAndNumberLine = addressLines[0];
            var lastSpaceIndex = streetAndNumberLine.LastIndexOf(' ');
            if (lastSpaceIndex > 0)
            {
                response.Street = streetAndNumberLine.Substring(0, lastSpaceIndex).Trim();
                response.Number = streetAndNumberLine.Substring(lastSpaceIndex + 1).Trim();
            }
            else
            {
                response.Street = streetAndNumberLine;
                response.Number = string.Empty;
            }

            response.Box = string.Empty;
            var postalAndCityLine = addressLines[1];

            var firstSpaceIndex = postalAndCityLine.IndexOf(' ');
            if (firstSpaceIndex > 0)
            {
                response.PostalCode = postalAndCityLine.Substring(0, firstSpaceIndex).Trim();
                response.Place = postalAndCityLine.Substring(firstSpaceIndex + 1).Trim();
            }
            else
            {
                response.PostalCode = postalAndCityLine;
                response.Place = string.Empty;
            }

            return response;
        }
    }
}