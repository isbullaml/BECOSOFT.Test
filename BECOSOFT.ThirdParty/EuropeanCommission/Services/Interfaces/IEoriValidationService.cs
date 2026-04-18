using BECOSOFT.Data.Services.Interfaces;
using BECOSOFT.ThirdParty.EuropeanCommission.Models;
using System.Threading.Tasks;

namespace BECOSOFT.ThirdParty.EuropeanCommission.Services.Interfaces {
    public interface IEoriValidationService : IBaseService {
        EoriValidationResponse Validate(EoriNumber eoriNumber);
        Task<EoriValidationResponse> ValidateAsync(EoriNumber eoriNumber);
    }
}