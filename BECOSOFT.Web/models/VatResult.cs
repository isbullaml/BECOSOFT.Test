using BECOSOFT.ThirdParty;
using BECOSOFT.Web.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ServiceModel.Channels;
namespace BECOSOFT.Web.Models
{
    public class VatResult
    {
        public VatResult(string name, string street, string number, string postalCode, string place, string countryCode)
        {
            var streetPart = $"{street} {number}".Trim();
            var placePart = $"{postalCode} {place}".Trim();
            var country = CountryHelper.GetCountryName(countryCode);

            var formatted = $"{name} / {streetPart}, {placePart}, {country}"
                .Replace("  ", " ")
                .Trim(' ', ',', '/');

            FullAddress = formatted;
        }
        public string FullAddress { get; set; }
    }
}