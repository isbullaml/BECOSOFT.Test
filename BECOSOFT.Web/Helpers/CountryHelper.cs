using System;
using System.Globalization;
using System.Linq;
namespace BECOSOFT.Web.Helpers
{
    public static class CountryHelper
    {
        public static string GetCountryName(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
                return string.Empty;

            try
            {
                var currentCulture = CultureInfo.CurrentUICulture;

                var region = CultureInfo
                    .GetCultures(CultureTypes.SpecificCultures)
                    .Select(c => new RegionInfo(c.Name))
                    .FirstOrDefault(r =>
                        r.TwoLetterISORegionName.Equals(countryCode, StringComparison.OrdinalIgnoreCase));

                return region?.DisplayName ?? countryCode;
            }
            catch
            {
                return countryCode;
            }
        }
    }

}