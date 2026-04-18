using System;
using System.Collections.Generic;

namespace BECOSOFT.Data.Collections {
    /// <summary>
    /// A readonly list with pages
    /// </summary>
    /// <typeparam name="T">The type of instances</typeparam>
    public interface IPagedList<T> : IReadOnlyList<T> {
        /// <summary>
        /// The total amount of pages
        /// </summary>
        int TotalPages { get; }
        /// <summary>
        /// The current page
        /// </summary>
        int CurrentPage { get; set; }
        /// <summary>
        /// The size of one page
        /// </summary>
        int PageSize { get; }
        /// <summary>
        /// The total count of items
        /// </summary>
        int TotalCount { get; }
        /// <summary>
        /// Value indicating whether the list has pages
        /// </summary>
        bool HasPages { get; }
        /// <summary>
        /// The list of all items
        /// </summary>
        List<T> Items { get; set; }
        /// <summary>
        /// The totalcount of unpaged items
        /// </summary>
        int TotalUnpagedCount { get; }
        /// <summary>
        /// Sets the page info
        /// </summary>
        /// <param name="currentPage">The current page</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="totalCount">The total count</param>
        void SetPageInfo(int currentPage, int pageSize, int totalCount);
        /// <summary>
        /// Converts the current pagedlist to a pagedlist of type U
        /// </summary>
        /// <typeparam name="TU">The type of U</typeparam>
        /// <param name="converter">The converter</param>
        /// <returns>The pagedlist of U</returns>
        IPagedList<TU> Convert<TU>(Func<T, TU> converter);
        /// <summary>
        /// Converts a pagedlist to a list
        /// </summary>
        /// <returns>A list of T</returns>
        List<T> ToList();
    }
}