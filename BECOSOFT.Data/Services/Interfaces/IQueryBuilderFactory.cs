using BECOSOFT.Data.Models;
using BECOSOFT.Data.Query;

namespace BECOSOFT.Data.Services.Interfaces {
    /// <summary>
    /// Factory for creating a <see cref="BaseQueryBuilder"/>
    /// </summary>
    internal interface IQueryBuilderFactory : IBaseService {
        /// <summary>
        /// Creates a <see cref="BaseQueryBuilder"/>
        /// </summary>
        /// <param name="type">The type of the query</param>
        /// <param name="info">The info for the query</param>
        /// <returns></returns>
        BaseQueryBuilder GetBuilder(QueryType type, QueryInfo info);
    }
}