using BECOSOFT.Data.Validation;
using System.Diagnostics;

namespace BECOSOFT.ThirdParty.EuropeanCommission.Models {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class VatNumberInfo {
        public VatNumber VatNumber { get; }
        public ViesValidationResponse ViesResponse { get; set; }
        public bool HasViesResponse => ViesResponse != null;
        public ValidationResult<VatNumber> ValidationResult { get; set; }
        public bool IsValid => (ValidationResult?.IsValid() ?? false) && (ViesResponse?.Valid ?? false);

        public VatNumberInfo(VatNumber vatNumber) {
            VatNumber = vatNumber;
        }

        private string DebuggerDisplay => $"{VatNumber?.CleanedCountryCode} {VatNumber?.VatNumberWithoutCountryCode} - IsValid? {IsValid}";
    }
}
