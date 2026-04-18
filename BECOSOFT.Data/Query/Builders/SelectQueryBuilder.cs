using BECOSOFT.Data.Models;
using BECOSOFT.Data.Repositories.Interfaces;
using System.Text;

namespace BECOSOFT.Data.Query.Builders {
    /// <summary>
    /// Query builder for SELECT queries
    /// </summary>
    internal class SelectQueryBuilder : SelectBaseQueryBuilder {
        public override QueryType Type => QueryType.Select;

        public SelectQueryBuilder(IJoinQueryBuilder joinQueryBuilder, IOfflineTableExistsRepository tableExistsRepository)
            : base(joinQueryBuilder, tableExistsRepository) {
        }

        protected override StringBuilder GenerateQuery() {
            return GenerateSelectQuery();
        }
    }
}