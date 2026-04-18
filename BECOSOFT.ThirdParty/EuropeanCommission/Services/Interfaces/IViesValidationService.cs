using BECOSOFT.Data.Services.Interfaces;
using BECOSOFT.ThirdParty.EuropeanCommission.Models;
using System.Threading.Tasks;

namespace BECOSOFT.ThirdParty.EuropeanCommission.Services.Interfaces {
    public interface IViesValidationService : IBaseService {
        ViesValidationResponse Validate(VatNumber vatNumber);
        Task<ViesValidationResponse> ValidateAsync(VatNumber vatNumber);
    }
}