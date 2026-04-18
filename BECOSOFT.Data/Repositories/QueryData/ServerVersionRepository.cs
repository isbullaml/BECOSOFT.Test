using BECOSOFT.Data.Context;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.QueryData;
using BECOSOFT.Data.Repositories.Interfaces.QueryData;
using BECOSOFT.Data.Services.Interfaces;
using System.Linq;

namespace BECOSOFT.Data.Repositories.QueryData {
    internal class ServerVersionRepository : BaseResultRepository<ServerVersion>, IServerVersionRepository {
        private readonly IDatabaseCommandFactory _databaseCommandFactory;
        internal ServerVersionRepository(IDbContextFactory dbContextFactory, 
                                         IDatabaseCommandFactory databaseCommandFactory)
            : base(dbContextFactory, databaseCommandFactory) {
            _databaseCommandFactory = databaseCommandFactory;
        }

        public ServerVersion GetVersion() {
            var parameterQuery = new ParametrizedQuery($" SELECT SERVERPROPERTY('productversion') AS ENT0_{Entity.GetColumn<ServerVersion>(r => r.VersionNumber)} ");
            var command = _databaseCommandFactory.Custom<ServerVersion>(parameterQuery);

            return Execute(Func, () => command.ToHashString());

            ServerVersion Func() {
                using (var context = DbContextFactory.CreateBaseResultContext()) {
                    var result = context.Query<ServerVersion>(command);
                    return result.FirstOrDefault();
                }
            }
        }
    }
}