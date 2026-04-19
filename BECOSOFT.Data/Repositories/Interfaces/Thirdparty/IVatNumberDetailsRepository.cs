using BECOSOFT.Data.Models.Thirdparty;
using BECOSOFT.Data.Repositories.Interfaces;

namespace BECOSOFT.Data.Repositories.Interfaces.Thirdparty {
    /// <summary>
    /// Repository for managing VAT number details
    /// </summary>
    public interface IVatNumberDetailsRepository : IBaseRepository {
        /// <summary>
        /// Gets VAT number details by VAT number
        /// </summary>
        /// <param name="vatNumber">The VAT number to search for (with country code)</param>
        /// <returns>The VAT number details or null if not found</returns>
        VatNumberDetails GetByVatNumber(string vatNumber);

        /// <summary>
        /// Saves or updates VAT number details
        /// </summary>
        /// <param name="details">The VAT number details to save</param>
        /// <returns>The saved entity with Id</returns>
        VatNumberDetails SaveOrUpdate(VatNumberDetails details);

        /// <summary>
        /// Checks if a VAT number should be refreshed based on last update date
        /// </summary>
        /// <param name="vatNumber">The VAT number to check</param>
        /// <param name="maxAgeDays">Maximum age in days before refresh is needed</param>
        /// <returns>True if refresh is needed, otherwise false</returns>
        bool NeedsRefresh(string vatNumber, int maxAgeDays = 30);
    }
}