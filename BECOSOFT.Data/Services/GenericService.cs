using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Data.Validation;

namespace BECOSOFT.Data.Services {
    public sealed class GenericService<T> : Service<T> where T : BaseEntity {
        internal GenericService(IRepository<T> repository, IValidator<T> validator) : base(repository, validator) {
        }
    }
}