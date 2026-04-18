using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Data.Extensions {
    public static class EnumerableExtensions {
        /// <summary>
        /// Get the identities of the base entities
        /// </summary>
        /// <typeparam name="T">Type of the sequence</typeparam>
        /// <param name="values">Source sequence of <see cref="BaseEntity"/> elements</param>
        /// <returns></returns>
        public static List<int> GetIDs<T>(this IEnumerable<T> values) where T : BaseEntity {
            return values.Select(GetID).ToList();
        }
        /// <summary>
        /// Enable property change tracking on each value in <see cref="values"/>.
        /// </summary>
        /// <typeparam name="T">Type of the sequence</typeparam>
        /// <param name="values">Source sequence of <see cref="BaseEntity"/> elements</param>
        /// <param name="includeLinkedEntities">When <see langword="true"/>, property change tracking will also be disabled on all linked entity and linked entities properties</param>
        /// <returns></returns>
        public static void TrackChanges<T>(this IEnumerable<T> values, bool includeLinkedEntities = true) where T : IDirty {
            foreach (var value in values) {
                value.TrackChanges(includeLinkedEntities);
            }
        }
        /// <summary>
        /// Discard the current tracked properties and disables further change tracking on each value in <see cref="values"/>.
        /// This does not reset the values of the properties.
        /// </summary>
        /// <typeparam name="T">Type of the sequence</typeparam>
        /// <param name="values">Source sequence of <see cref="BaseEntity"/> elements</param>
        /// <param name="includeLinkedEntities">When <see langword="true"/>, property change tracking will also be disabled on all linked entity and linked entities properties</param>
        /// <returns></returns>
        public static void DiscardTrackedChanges<T>(this IEnumerable<T> values, bool includeLinkedEntities = true) where T : IDirty {
            foreach (var value in values) {
                value.DiscardTrackedChanges(includeLinkedEntities);
            }
        }

        private static int GetID(BaseEntity entity) {
            return entity.Id;
        }

        /// <summary>
        /// Converts a sequence to an <see cref="ObserverList{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of the sequence</typeparam>
        /// <param name="values">Source sequence</param>
        /// <returns>Returns an <see cref="ObserverList{T}"/> from the source sequence.</returns>
        public static ObserverList<T> ToObserverList<T>(this IEnumerable<T> values) {
            return new ObserverList<T>(values);
        }

        /// <summary>
        /// Transforms an <see cref="source"/> collection to a dictionary using the <see cref="BaseEntity.Id"/> property as key and <see cref="BaseEntity"/> as value.
        /// </summary>
        /// <typeparam name="T">Type of the collection</typeparam>
        /// <param name="source">The collection to convert to a <see cref="Dictionary{TKey,TValue}"/>.</param>
        /// <returns>Returns a <see cref="Dictionary{TKey,TValue}"/> based on the original <see cref="source"/>.</returns>
        public static Dictionary<int, T> ToDictionary<T>(this IEnumerable<T> source) where T : BaseEntity {
            var entities = source.ToSafeList();
            return entities.ToDictionary(s => s.Id, s => s);
        }

        /// <summary>
        /// <para>
        /// Placeholder function to generate a contains (gets transformed to SQL exists).
        /// </para>
        /// <para>
        /// This function is not ment to actually be used. It's a placeholder for query generation
        /// </para>
        /// </summary>
        /// <typeparam name="TBulkCopyable"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Contains<TBulkCopyable, T>(this IEnumerable<TBulkCopyable> values, T value) where TBulkCopyable : IBulkCopyable where T : IEntity {
            throw new NotImplementedException("This function is not ment to actually be used. It's a placeholder for query generation"); 
        }
    }
}