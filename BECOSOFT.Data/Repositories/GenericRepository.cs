using BECOSOFT.Data.Context;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Services.Interfaces;

namespace BECOSOFT.Data.Repositories {
    public sealed class GenericRepository<T> : Repository<T> where T : BaseEntity {
        public GenericRepository(IDbContextFactory dbContextFactory,
                                 IDatabaseCommandFactory databaseCommandFactory) : base(dbContextFactory, databaseCommandFactory) {
        }
    }
}