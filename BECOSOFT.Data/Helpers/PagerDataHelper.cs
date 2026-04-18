using BECOSOFT.Data.Extensions;
using BECOSOFT.Data.Models;
using BECOSOFT.Utilities.Extensions.Collections;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Data.Helpers {
    public static class PagerDataHelper {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        public static List<T> Sort<T>(IEnumerable<T> data, PagerData<T> pagerData = null) {
            var originalList = data.ToSafeList();
            if (pagerData == null) {
                return originalList;
            }

            try {
                var queryable = originalList.AsQueryable();
                for (var i = 0; i < pagerData.OrderBy.Count; i++) {
                    var orderBy = pagerData.OrderBy[i];
                    queryable = i == 0 ? queryable.OrderBy(orderBy) : ((IOrderedQueryable<T>)queryable).ThenBy(orderBy);
                }

                return queryable.ToSafeList();
            } catch (Exception e) {
               Logger.Error(e, "Error trying to sort on this expression: {0}", string.Join(", ", pagerData.OrderBy.Select(o => $"{o.OrderByExpression.Simplify()} (Asc: {o.IsAsc})")));
            }
            return originalList;
        }
    }
}
