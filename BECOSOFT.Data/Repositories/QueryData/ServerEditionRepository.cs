using BECOSOFT.Data.Context;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.QueryData;
using BECOSOFT.Data.Repositories.Interfaces.QueryData;
using BECOSOFT.Data.Services.Interfaces;
using System.Linq;

namespace BECOSOFT.Data.Repositories.QueryData {
    internal class ServerEditionRepository : BaseResultRepository<ServerEdition>, IServerEditionRepository {
        private readonly IDatabaseCommandFactory _databaseCommandFactory;
        internal ServerEditionRepository(IDbContextFactory dbContextFactory, 
                                         IDatabaseCommandFactory databaseCommandFactory) 
            : base(dbContextFactory, databaseCommandFactory) {
            _databaseCommandFactory = databaseCommandFactory;
        }

        public ServerEdition GetEdition() {
            var parameterQuery = new ParametrizedQuery($" SELECT SERVERPROPERTY('edition') AS ENT0_{Entity.GetColumn<ServerEdition>(r => r.EditionString)} ");
            var command = _databaseCommandFactory.Custom<ServerEdition>(parameterQuery);

            return Execute(Func, () => command.ToHashString());

            ServerEdition Func() {
                using (var context = DbContextFactory.CreateBaseResultContext()) {
                    var result = context.Query<ServerEdition>(command);
                    return result.FirstOrDefault();
                }
            }
        }
    }
}