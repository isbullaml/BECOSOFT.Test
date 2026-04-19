using BECOSOFT.Data.Services.Interfaces;
using BECOSOFT.Data.Validation;
using BECOSOFT.ThirdParty.EuropeanCommission.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BECOSOFT.ThirdParty.EuropeanCommission.Services.Interfaces {
    public interface IVatNumberValidationService : IBaseService {
        VatNumberInfo GetVatNumberInfo(VatNumber vatNumber);
        Task<VatNumberInfo> GetVatNumberInfoAsync(VatNumber vatNumber);
        List<VatNumberInfo> GetVatNumberInfo(IEnumerable<VatNumber> vatNumbers);
        ValidationResult<VatNumber> Validate(VatNumber vatNumber);
        MultiValidationResult<VatNumber> Validate(IEnumerable<VatNumber> vatNumbers);
    }
}