using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Context;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Data.Services.Interfaces;
using System;

namespace BECOSOFT.Data.Repositories {
    public abstract class PrimitiveRepository : IPrimitiveRepository {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly IDatabaseCommandFactory _databaseCommandFactory;

        protected PrimitiveRepository(IDbContextFactory dbContextFactory,
                                      IDatabaseCommandFactory databaseCommandFactory) {
            _dbContextFactory = dbContextFactory;
            _databaseCommandFactory = databaseCommandFactory;
        }

        public IPagedList<TProperty> Query<TProperty>(ParametrizedQuery query) where TProperty : IConvertible {
            var command = _databaseCommandFactory.Custom<TProperty>(query);
            using (var context = GetContext()) {
                return context.QueryConvertible<TProperty>(command);
            }
        }

        public IBaseEntityDbContext GetContext() {
            return _dbContextFactory.CreateBaseEntityContext();
        }
    }
}