using BECOSOFT.Data.Context;
using BECOSOFT.Data.Services.Interfaces;
using BECOSOFT.ThirdParty.EuropeanCommission.Models;
using BECOSOFT.ThirdParty.EuropeanCommission.Repositories.Interfaces;
using System;
using System.Linq;

namespace BECOSOFT.ThirdParty.EuropeanCommission.Repositories {
    public class VatNumberDetailsRepository : BECOSOFT.Data.Repositories.Repository<VatNumberDetails>, IVatNumberDetailsRepository {
        public VatNumberDetailsRepository(IDbContextFactory dbContextFactory,
                                          IDatabaseCommandFactory databaseCommandFactory)
            : base(dbContextFactory, databaseCommandFactory) {
            IsCachingEnabled = true;
        }

        public VatNumberDetails GetByVatNumber(string vatNumber) {
            if (string.IsNullOrWhiteSpace(vatNumber)) return null;
            var upper = vatNumber.ToUpperInvariant();
            return GetByProperty(x => x.VatNumber == upper).FirstOrDefault();
        }

        public VatNumberDetails SaveOrUpdate(VatNumberDetails details) {
            if (details == null) throw new ArgumentNullException(nameof(details));
            details.VatNumber = details.VatNumber?.ToUpperInvariant();
            details.LastUpdated = DateTime.Now;

            var existing = GetByVatNumber(details.VatNumber);
            if (existing != null) {
                details.Id = existing.Id;
                details.IsDirty = true;
            }
            Save(details);

            if (UseCaching) { RefreshCache(); }
            return details;
        }

        public bool NeedsRefresh(string vatNumber, int maxAgeDays = 30) {
            var details = GetByVatNumber(vatNumber);
            if (details == null) return true;
            return (DateTime.Now - details.LastUpdated).TotalDays > maxAgeDays;
        }
    }
}
