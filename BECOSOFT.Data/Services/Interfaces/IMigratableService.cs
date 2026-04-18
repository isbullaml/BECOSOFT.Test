using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Validation;
using System.Collections.Generic;

namespace BECOSOFT.Data.Services.Interfaces {
    public interface IMigratableService<T> where T : BaseEntity {
        MultiValidationResult<T> SaveWithMigration(IEnumerable<T> entities);
        ValidationResult<T> SaveWithMigration(T entity);
        void SetProgressService(IProgressService progressService);
    }
}
