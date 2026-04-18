using BECOSOFT.Data.Validation;
using BECOSOFT.ThirdParty.EuropeanCommission.Models;
using BECOSOFT.ThirdParty.EuropeanCommission.Services.Interfaces;
using BECOSOFT.ThirdParty.EuropeanCommission.Validators;
using BECOSOFT.Utilities.Extensions.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.ThirdParty.EuropeanCommission.Services {
    public sealed class VatNumberValidationService : IVatNumberValidationService {
        private readonly IViesValidationService _viesValidationService;
        private readonly IValidator<VatNumber> _validator;

        public VatNumberValidationService(IValidator<VatNumber> validator, IViesValidationService viesValidationService) {
            _validator = validator;
            _viesValidationService = viesValidationService;
        }


        public VatNumberInfo GetVatNumberInfo(VatNumber vatNumber) {
            var vatNumberInfo = new VatNumberInfo(vatNumber) {
                ValidationResult = Validate(vatNumber)
            };
            if (vatNumberInfo.ValidationResult.IsValid() && VatNumberValidator.IsEuropean(vatNumberInfo.VatNumber)) {
                vatNumberInfo.ViesResponse = _viesValidationService.Validate(vatNumber);
                vatNumberInfo.VatNumber.ValidatedVatNumber = vatNumberInfo.VatNumber.CleanedCountryCode + vatNumberInfo.ViesResponse.VatNumber;
            }
            return vatNumberInfo;
        }

        public List<VatNumberInfo> GetVatNumberInfo(IEnumerable<VatNumber> vatNumbers) {
            var vatNumberList = vatNumbers.ToSafeList();
            var multiValidationResult = Validate(vatNumberList);
            var result = multiValidationResult.Results.Select(validationResult => new VatNumberInfo(validationResult.ValidatedEntity) { ValidationResult = validationResult })
                                              .ToList();

            foreach (var vatNumberInfo in result) {
                if (!vatNumberInfo.ValidationResult.IsValid() || !VatNumberValidator.IsEuropean(vatNumberInfo.VatNumber)) {
                    continue;
                }
                vatNumberInfo.ViesResponse = _viesValidationService.Validate(vatNumberInfo.VatNumber);
            }

            return result;
        }

        public ValidationResult<VatNumber> Validate(VatNumber vatNumber) {
            var vatNumbers = new List<VatNumber> { vatNumber };
            var result = Validate(vatNumbers);
            return result.Results.First();
        }

        public MultiValidationResult<VatNumber> Validate(IEnumerable<VatNumber> vatNumbers) {
            return _validator.Validate(vatNumbers);
        }
    }
}
