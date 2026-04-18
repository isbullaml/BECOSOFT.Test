using System;
using System.Collections.Generic;

namespace BECOSOFT.Utilities.Comparers {
    /// <summary>
    /// Use the <see cref="EqualityComparerFactory"/> to create an <see cref="IEqualityComparer{T}"/> based on a given equals function.
    /// </summary>
    public static class EqualityComparerFactory {
        /// <summary>
        /// Create an <see cref="IEqualityComparer{T}"/> from an <see cref="equals"/> function. The <see cref="T:T.GetHashCode()"/> function from the type is used.
        /// </summary>
        /// <typeparam name="T">Type of the comparer</typeparam>
        /// <param name="equals">Equals function</param>
        /// <returns></returns>
        public static IEqualityComparer<T> Create<T>(Func<T, T, bool> equals) {
            return Create(equals, null);
        }

        /// <summary>
        /// Create an <see cref="IEqualityComparer{T}"/> from the provided <see cref="equals"/> and <see cref="hash"/> functions.
        /// </summary>
        /// <typeparam name="T">Type of the comparer</typeparam>
        /// <param name="equals">Equals function</param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static IEqualityComparer<T> Create<T>(Func<T, T, bool> equals, Func<T, int> hash) {
            if (equals == null) {
                throw new ArgumentNullException(nameof(equals));
            }
            return new EqualityComparerImpl<T>(equals, hash);
        }

        private class EqualityComparerImpl<T> : IEqualityComparer<T> {
            private readonly Func<T, T, bool> _equals;
            private readonly Func<T, int> _hash;

            public EqualityComparerImpl(Func<T, T, bool> equals, Func<T, int> hash) {
                _equals = equals;
                _hash = hash ?? (obj => obj.GetHashCode());
            }

            public bool Equals(T x, T y) => _equals(x, y);

            public int GetHashCode(T obj) => _hash(obj);
        }
    }
}

// Inspiration: https://stackoverflow.com/questions/3189861/pass-a-lambda-expression-in-place-of-icomparer-or-iequalitycomparer-or-any-singl