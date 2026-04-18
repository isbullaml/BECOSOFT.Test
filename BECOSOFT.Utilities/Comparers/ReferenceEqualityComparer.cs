using System.Collections.Generic;
using System.Runtime.CompilerServices;

// https://stackoverflow.com/questions/8946790/how-to-use-an-objects-identity-as-key-for-dictionaryk-v

namespace BECOSOFT.Utilities.Comparers {
    /// <summary>
    /// An <see cref="IEqualityComparer{T}"/> that uses reference equality (<see cref="object.ReferenceEquals(object?, object?)"/>)
    /// instead of value equality (<see cref="object.Equals(object?)"/>) when comparing two object instances.
    /// </summary>
    /// <remarks>
    /// The <see cref="ReferenceEqualityComparer{T}"/> type cannot be instantiated. Instead, use the <see cref="Instance"/> property
    /// to access the singleton instance of this type.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>
        where T : class {
        
        /// <summary>
        /// Gets the default instance of the
        /// <see cref="ReferenceEqualityComparer{T}"/> class.
        /// </summary>
        /// <value>A <see cref="ReferenceEqualityComparer{T}"/> instance.</value>
        public static ReferenceEqualityComparer<T> Instance { get; } = new ReferenceEqualityComparer<T>();

        /// <summary>
        /// Determines whether two object references refer to the same object instance.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>
        /// <see langword="true"/> if both <paramref name="left"/> and <paramref name="right"/> refer to the same object instance
        /// or if both are <see langword="null"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This API is a wrapper around <see cref="object.ReferenceEquals(object?, object?)"/>.
        /// It is not necessarily equivalent to calling <see cref="object.Equals(object?, object?)"/>.
        /// </remarks>
        public bool Equals(T left, T right) => ReferenceEquals(left, right);

        /// <summary>
        /// Returns a hash code for the specified object. The returned hash code is based on the object
        /// identity, not on the contents of the object.
        /// </summary>
        /// <param name="value">The object for which to retrieve the hash code.</param>
        /// <returns>A hash code for the identity of <paramref name="value"/>.</returns>
        /// <remarks>
        /// This API is a wrapper around <see cref="RuntimeHelpers.GetHashCode(object)"/>.
        /// It is not necessarily equivalent to calling <see cref="object.GetHashCode()"/>.
        /// </remarks>
        public int GetHashCode(T value) => RuntimeHelpers.GetHashCode(value);
    }
}