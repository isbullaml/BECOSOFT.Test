using BECOSOFT.Data.Models.Thirdparty;
using BECOSOFT.Data.Repositories.Interfaces.Thirdparty;
using BECOSOFT.ThirdParty.EuropeanCommission.Models;
using BECOSOFT.ThirdParty.EuropeanCommission.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace BECOSOFT.ThirdParty.EuropeanCommission.Services {
    public class VatNumberCacheService : IVatNumberCacheService {
        private const int MaxAgeDays = 30;

        private readonly IVatNumberDetailsRepository _repository;
        private readonly IVatNumberValidationService _vatNumberValidationService;

        public VatNumberCacheService(
            IVatNumberDetailsRepository repository,
            IVatNumberValidationService vatNumberValidationService) {
            _repository = repository;
            _vatNumberValidationService = vatNumberValidationService;
        }

        public async Task<VatNumberInfo> GetVatNumberInfoAsync(VatNumber vatNumber) {
            var normalizedVat = vatNumber.CleanedVatNumber?.ToUpperInvariant()
                                ?? vatNumber.VatIdentificationNumber?.ToUpperInvariant();

            var existing = _repository.GetByVatNumber(normalizedVat);
            var isFresh = existing != null && (DateTime.Now - existing.LastUpdated).TotalDays <= MaxAgeDays;

            if (isFresh) {
                return MapToVatNumberInfo(vatNumber, existing);
            }

            var vatNumberInfo = await _vatNumberValidationService.GetVatNumberInfoAsync(vatNumber);
            _repository.SaveOrUpdate(MapToDetails(vatNumberInfo, normalizedVat));
            return vatNumberInfo;
        }

        private VatNumberInfo MapToVatNumberInfo(VatNumber vatNumber, VatNumberDetails details) {
            return new VatNumberInfo(vatNumber) {
                ValidationResult = _vatNumberValidationService.Validate(vatNumber),
                ViesResponse = ViesValidationResponse.FromDetails(details)
            };
        }

        private static VatNumberDetails MapToDetails(VatNumberInfo info, string normalizedVat) {
            var vies = info.ViesResponse;
            return new VatNumberDetails {
                VatNumber   = info.VatNumber.ValidatedVatNumber?.ToUpperInvariant() ?? normalizedVat,
                CountryCode = vies?.CountryCode ?? info.VatNumber.CleanedCountryCode,
                Name        = vies?.Name,
                Address     = vies?.Address?.Address,
                Street      = vies?.Address?.Street,
                PostalCode  = vies?.Address?.PostalCode,
                Place       = vies?.Address?.Place,
                Number      = vies?.Address?.Number,
                Box         = vies?.Address?.Box,
                Province    = vies?.Address?.Province,
                AddressLine = vies?.Address?.AddressLine,
                IsValid     = info.IsValid,
                LastUpdated = DateTime.Now
            };
        }
    }
}
