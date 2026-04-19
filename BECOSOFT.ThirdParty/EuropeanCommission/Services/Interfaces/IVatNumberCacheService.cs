using BECOSOFT.Data.Services.Interfaces;
using BECOSOFT.ThirdParty.EuropeanCommission.Models;
using System.Threading.Tasks;

namespace BECOSOFT.ThirdParty.EuropeanCommission.Services.Interfaces {
    public interface IVatNumberCacheService : IBaseService {
        Task<VatNumberInfo> GetVatNumberInfoAsync(VatNumber vatNumber);
    }
}
