using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Models {
    /// <summary>
    /// Class containing the info about the pagination
    /// </summary>
    public class PagerData<T> {
        /// <summary>
        /// The amount of items to skip
        /// </summary>
        public int? Skip { get; }
        /// <summary>
        /// The amount of items to take
        /// </summary>
        public int? Take { get; }
        /// <summary>
        /// The search text for the query
        /// </summary>
        public string SearchText { get; set; }

        public bool Distinct { get; set; }
        internal List<OrderDefinition<T>> OrderBy { get; }

        public PagerData(int? skip = null, int? take = null, string searchText = null, bool distinct = false) {
            Skip = skip;
            Take = take;
            SearchText = searchText;
            Distinct = distinct;
            OrderBy = new List<OrderDefinition<T>>(0);
        }

        public void AddOrderBy(Expression<Func<T, object>> orderByExpression, bool isAsc = true) => AddOrderBy(new OrderDefinition<T>(orderByExpression, isAsc));

        public void AddOrderBy(OrderDefinition<T> orderExpression) {
            if (orderExpression == null) { return;}
            OrderBy.Add(orderExpression);
        }

        public void AddOrderBy(int position, Expression<Func<T, object>> orderByExpression, bool isAsc = true) => AddOrderBy(position, new OrderDefinition<T>(orderByExpression, isAsc));

        public void AddOrderBy(int position, OrderDefinition<T> orderExpression) => OrderBy.Insert(position, orderExpression);

        public void RemoveOrderBy(OrderDefinition<T> orderExpression) => OrderBy.Remove(orderExpression);

        public void RemoveOrderBy(int position) => OrderBy.RemoveAt(position);

        /// <summary>
        /// Checks if a <see cref="PagerData{T}"/> is equal to the current one
        /// </summary>
        /// <param name="other">The second pagerdata</param>
        /// <returns>True if the are equal, false if not</returns>
        protected bool Equals(PagerData<T> other) {
            return Skip == other.Skip && Take == other.Take && Distinct == other.Distinct &&
                   string.Equals(SearchText, other.SearchText) && OrderBy.SequenceEqual(other.OrderBy);
        }

        /// <inheritdoc />
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((PagerData<T>) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() {
            unchecked {
                var hashCode = Skip ?? 0;
                hashCode = (hashCode * 397) ^ (Take ?? 0);
                hashCode = (hashCode * 397) ^ Distinct.GetHashCode();
                hashCode = (hashCode * 397) ^ (SearchText?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (OrderBy?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public bool HasSkipAndTake => Take.HasValue && Skip.HasValue;

        /// <inheritdoc />
        public override string ToString() {
            return "S" + Skip + "T" + Take + "D" + Distinct + "ST" + (SearchText ?? "") + "O" + OrderBy.Count;
        }
    }

    public static class PagerData {
        public static PagerData<T> CreateWithOrder<T>(Expression<Func<T, object>> orderByExpression, bool isAsc = true) {
            var pagerData = new PagerData<T>();
            pagerData.AddOrderBy(orderByExpression, isAsc);
            return pagerData;
        }
    }
}