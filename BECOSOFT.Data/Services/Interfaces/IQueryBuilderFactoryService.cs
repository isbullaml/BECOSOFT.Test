using BECOSOFT.Data.Models;
using BECOSOFT.Data.Query;

namespace BECOSOFT.Data.Services.Interfaces {
    internal interface IQueryBuilderFactoryService : IKeyedFactoryService<QueryType> {
        /// <summary>
        /// The info to build the query
        /// </summary>
        QueryInfo Info { get; }

        BaseQueryBuilder Initialize(QueryInfo info);

        /// <summary>
        /// Prepares a command and sets the parameters
        /// </summary>
        /// <param name="command">The command to prepare</param>
        void PrepareCommand(DatabaseCommand command);
    }
}