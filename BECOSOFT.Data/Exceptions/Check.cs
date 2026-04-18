using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Extensions;
using System;

namespace BECOSOFT.Data.Exceptions {
    /// <summary>
    /// Class for performing common checks
    /// </summary>
    public static class Check {
        /// <summary>
        /// Performs a check to see if the <see cref="tablePart"/> is missing for the provided type.
        /// </summary>
        /// <param name="tablePart">Table part of the <see cref="TableDefiningEntity"/></param>
        /// <exception cref="ArgumentException">Throws if the provided type is a <see cref="TableConsumingEntity{T}"/> or <see cref="TableConsumingResult{T}"/> with a non null or empty <see cref="tablePart"/>.</exception>
        public static void IsValidTableConsuming<T>(string tablePart) {
            if (Check<T>.IsTableConsuming && tablePart.IsNullOrEmpty()) {
                throw new ArgumentException(Resources.Error_MissingTablePart);
            }
        }

        /// <summary>
        /// Performs a check to see if the <see cref="tablePart"/> is missing for the provided type.
        /// </summary>
        /// <param name="typeToCheck">Type to check</param>
        /// <param name="tablePart">Table part of the <see cref="TableDefiningEntity"/></param>
        /// <exception cref="ArgumentException">Throws if the provided type is a <see cref="TableConsumingEntity{T}"/> or <see cref="TableConsumingResult{T}"/> with a non null or empty <see cref="tablePart"/>.</exception>
        public static void IsValidTableConsuming(Type typeToCheck, string tablePart) {
            if (IsTableConsuming(typeToCheck) && tablePart.IsNullOrEmpty()) {
                throw new ArgumentException(Resources.Error_MissingTablePart);
            }
        }

        /// <summary>
        /// Performs a check to see if the type is not a <see cref="TableConsumingEntity{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Throws if the provided type is a <see cref="TableConsumingEntity{T}"/> or <see cref="TableConsumingResult{T}"/>.</exception>

        public static void IsNotTableConsuming<T>() {
            if (Check<T>.IsTableConsuming) {
                throw new ArgumentException(Resources.Error_InvalidType_TableConsuming);
            }
        }

        /// <summary>
        /// Performs a check to see if the type is a <see cref="TableConsumingEntity{T}"/> or <see cref="TableConsumingResult{T}"/>.
        /// </summary>

        public static bool IsTableConsuming(Type type) {
            return type.IsSubclassOfRawGeneric(typeof(TableConsumingEntity<>))
                || type.IsSubclassOfRawGeneric(typeof(TableConsumingResult<>));
        }
    }

    internal static class Check<T> {
        internal static bool IsTableConsuming
            = typeof(T).IsSubclassOfRawGeneric(typeof(TableConsumingEntity<>))
              || typeof(T).IsSubclassOfRawGeneric(typeof(TableConsumingResult<>));
    }
}