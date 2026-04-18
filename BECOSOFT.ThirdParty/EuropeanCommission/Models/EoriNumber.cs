using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace BECOSOFT.ThirdParty.EuropeanCommission.Models {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class EoriNumber : IValidatable {
        [Required]
        public string Eori { get; }

        /// <summary>
        /// Country code retrieved from <see cref="Eori"/>
        /// </summary>
        public string CountryCode => GetCountryPart(Eori);

        /// <summary>
        /// Contains the <see cref="Eori"/>, cleaned from invalid characters
        /// </summary>
        public string CleanedEori { get; set; }

        /// <summary>
        /// Country code retrieved from <see cref="CleanedEori"/>
        /// </summary>
        public string CleanedCountryCode => GetCountryPart(CleanedEori);
        
        /// <summary>
        /// Contains the EORI number validated by the EORI validation web service. If empty, the EORI number wasn't validated by the EORI validation web service.
        /// </summary>
        public string ValidatedEori { get; set; }

        /// <summary>
        /// Contains the <see cref="CleanedEori"/> without the country code.
        /// </summary>
        public string EoriWithoutCountryCode => HasCountryCode ? CleanedEori?.Replace(CleanedCountryCode, "").Trim() ?? "" : "";

        public EoriNumber(string eori) {
            Eori = eori?.Trim().ToUpperInvariant() ?? "";
        }

        public bool HasCountryCode => !CleanedCountryCode.IsNullOrWhiteSpace();

        private static string GetCountryPart(string eoriNumber) {
            var countryCode = !string.IsNullOrWhiteSpace(eoriNumber) && eoriNumber.Length > 1
                ? eoriNumber.Substring(0, 2)
                : string.Empty;
            int parseResult;
            return int.TryParse(countryCode, out parseResult) ? string.Empty : countryCode;
        }

        private string DebuggerDisplay => $"{Eori}";
    }
}