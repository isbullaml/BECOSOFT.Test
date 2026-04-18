using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Query;
using BECOSOFT.Utilities.Annotations;

namespace BECOSOFT.Data.Services {
    [UsedImplicitly]
    public abstract class BaseService<T> where T : IEntity {
        protected static QueryExpression<T> GetQueryExpression(PagerData<T> pagerData = null) {
            var queryExpression = new QueryExpression<T>();
            queryExpression.SetFromPagerData(pagerData);
            return queryExpression;
        }
    }
}