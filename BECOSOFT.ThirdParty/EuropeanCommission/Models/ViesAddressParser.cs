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

            return response;
        }
    }
}