using BECOSOFT.Data.Context;
using BECOSOFT.Data.Helpers;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.QueryData;
using BECOSOFT.Data.Repositories.Interfaces.QueryData;
using BECOSOFT.Data.Services.Interfaces;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;

namespace BECOSOFT.Data.Repositories.QueryData {
    internal class ServerQueryRepository : BaseResultRepository<ServerQueryResult>, IServerQueryRepository {
        internal ServerQueryRepository(IDbContextFactory dbContextFactory,
                                       IDatabaseCommandFactory databaseCommandFactory) 
            : base(dbContextFactory, databaseCommandFactory) {
        }

        public bool LinkedServerExists(string linkedServer) {
            var cleanServer = TableHelper.Clean(linkedServer);
            if (cleanServer.IsNullOrWhiteSpace()) { return false; }
            var parameterQuery = new ParametrizedQuery($"SELECT CASE WHEN EXISTS((SELECT 1 FROM sys.servers WHERE name = @LinkedServerName AND [is_linked] = 1)) THEN 1 ELSE 0 END AS ENT0_{GetColumn(r => r.Exists)}");
            parameterQuery.AddParameter("@LinkedServerName", cleanServer);
            return ExecuteDatabaseQuery(parameterQuery);
        }

        private bool ExecuteDatabaseQuery(ParametrizedQuery parameterQuery) {
            var command = DatabaseCommandFactory.Custom<ServerQueryResult>(parameterQuery);

            return Execute(Func, () => command.ToHashString());

            bool Func() {
                using (var context = DbContextFactory.CreateBaseResultContext()) {
                    var result = context.Query<ServerQueryResult>(command);
                    return result.HasAny() && result[0].Exists;
                }
            }
        }
    }
}