using BECOSOFT.Data.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BECOSOFT.Data.Collections {
    /// <inheritdoc />
    [DebuggerDisplay("{ElementType} Total: {TotalCount}, Current: {_items.Count}")]
    public class PagedList<T> : IPagedList<T> {
        /// <inheritdoc />
        public int TotalPages { get; private set; }
        /// <inheritdoc />
        public int CurrentPage { get; set; }
        /// <inheritdoc />
        public int PageSize { get; private set; }
        /// <summary>
        /// The type of T
        /// </summary>
        public Type ElementType => typeof(T);
        /// <inheritdoc />
        public int TotalCount {
            get {
                if (CurrentPage == 0 && PageSize == 0 && TotalPages == 0) {
                    return Items.Count;
                }
                return TotalUnpagedCount;
            }
        }

        private List<T> _items;
        /// <inheritdoc />
        public List<T> Items {
            get { return _items; }
            set {
                if (_items != null) {
                    if (_items.Count != 0 && _items.Count != value.Count) {
                        throw new InvalidListCountException();
                    }
                } else if (value == null) {
                    throw new ArgumentNullException();
                }
                _items = value;
            }
        }

        public PagedList(IEnumerable<T> items = null) {
            Items = items?.ToList() ?? new List<T>(0);
            var pagedList = items as IPagedList<T>;
            if (pagedList == null || Items.Count != pagedList.Items.Count) {
                return;
            }
            CurrentPage = pagedList.CurrentPage;
            PageSize = pagedList.PageSize;
            TotalUnpagedCount = pagedList.TotalUnpagedCount;
            TotalPages = pagedList.TotalPages;
        }

        /// <inheritdoc />
        public void SetPageInfo(int currentPage, int pageSize, int totalCount) {
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalUnpagedCount = totalCount;
            if (PageSize != 0) {
                TotalPages = (int) Math.Ceiling(totalCount / (double) PageSize);
            }
        }

        /// <inheritdoc />
        public IPagedList<TU> Convert<TU>(Func<T, TU> converter) {
            var pagedList = new PagedList<TU>();
            pagedList.SetPageInfo(CurrentPage, PageSize, TotalCount);
            pagedList.Items = Items.Select(converter).ToList();
            return pagedList;
        }

        /// <inheritdoc />
        public bool HasPages => TotalCount != 0 && PageSize != 0;

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator() {
            return _items.GetEnumerator();
        }

        /// <inheritdoc />
        public List<T> ToList() {
            return new List<T>(_items);
        }

        /// <inheritdoc />
        public int TotalUnpagedCount { get; private set; }

        #region IReadonlyList members
        /// <inheritdoc />
        public int Count => _items.Count;
        /// <inheritdoc />
        public T this[int index] => _items[index];

        #endregion
    }
}