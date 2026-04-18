using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace BECOSOFT.ThirdParty.EuropeanCommission.Models {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class VatNumber : IValidatable {
        /// <summary>
        /// VAT identification number (initialized by the constructor)
        /// </summary>
        [Required]
        public string VatIdentificationNumber { get; }

        /// <summary>
        /// Country code retrieved from <see cref="VatIdentificationNumber"/>
        /// </summary>
        public string CountryCode => GetCountryPart(VatIdentificationNumber);

        /// <summary>
        /// Contains the <see cref="VatIdentificationNumber"/>, cleaned from invalid characters
        /// </summary>
        public string CleanedVatNumber { get; set; }

        /// <summary>
        /// Country code retrieved from <see cref="CleanedVatNumber"/>
        /// </summary>
        public string CleanedCountryCode => GetCountryPart(CleanedVatNumber);

        /// <summary>
        /// Contains the country code and the VAT number validated by the VIES web service. If empty, the VAT number wasn't validated by the VIES web service.
        /// </summary>
        public string ValidatedVatNumber { get; set; }

        /// <summary>
        /// Contains the <see cref="CleanedVatNumber"/> without the country code.
        /// </summary>
        public string VatNumberWithoutCountryCode => HasCountryCode ? CleanedVatNumber?.Replace(CleanedCountryCode, "").Trim() ?? "" : "";

        public VatNumber(string vatNumber) {
            VatIdentificationNumber = vatNumber?.Trim().ToUpperInvariant() ?? "";
        }

        public bool HasCountryCode => !CleanedCountryCode.IsNullOrWhiteSpace();

        private static string GetCountryPart(string vatNumber) {
            var countryCode = !string.IsNullOrWhiteSpace(vatNumber) && vatNumber.Length > 1
                ? vatNumber.Substring(0, 2)
                : string.Empty;
            int parseResult;
            return int.TryParse(countryCode, out parseResult) ? string.Empty : countryCode;
        }

        private string DebuggerDisplay => $"{VatIdentificationNumber}";
    }
}