using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.ThirdParty.EuropeanCommission.Models;

namespace BECOSOFT.ThirdParty.EuropeanCommission.Repositories.Interfaces {
    public interface IVatNumberDetailsRepository : IBaseRepository {
        VatNumberDetails GetByVatNumber(string vatNumber);
        VatNumberDetails SaveOrUpdate(VatNumberDetails details);
        bool NeedsRefresh(string vatNumber, int maxAgeDays = 30);
    }
}
