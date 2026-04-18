using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Data.Validation;

namespace BECOSOFT.Data.Services {
    public sealed class GenericTableService<T, TDefining> : TableService<T, TDefining> 
        where T : TableConsumingEntity<TDefining>
        where TDefining : TableDefiningEntity {
        internal GenericTableService(IRepository<T> repository,
                                     IRepository<TDefining> definingRepository,
                                     IValidator<T, TDefining> validator) 
            : base(repository, definingRepository, validator) {
        }
    }
}