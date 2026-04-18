using BECOSOFT.Utilities.Exceptions;
using BECOSOFT.Utilities.Extensions.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace BECOSOFT.Utilities.Helpers {
    [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
    public static class MathHelpers {
        /// <summary>
        /// Finds the smallest positive integer that is divisible by all integers in the provided collection.
        /// </summary>
        /// <param name="numbers">The collection to find the least common multiple for</param>
        /// <returns>The smallest positive integer that is divisible by all integers in the provided collection</returns>
        public static long GetLeastCommonMultiple(IEnumerable<int> numbers) {
            var numberList = numbers.ToSafeList();
            if (numberList.IsEmpty()) {
                throw new EmptyCollectionException();
            }
            var max = numberList.Max();
            var currentFactor = 2;
            var result = 1L;
            while (currentFactor < max) {
                var indices = numberList.FindIndices(i => i % currentFactor == 0);
                if (indices.Count >= 2) {
                    foreach (var index in indices) {
                        numberList[index] /= currentFactor;
                    }
                    result *= currentFactor;
                } else {
                    currentFactor++;
                }
            }
            var res = numberList.Aggregate(result, (temp, value) => temp * value);
            return res;
        }
        /// <summary>
        /// Finds the smallest positive long that is divisible by all longs in the provided collection.
        /// </summary>
        /// <param name="numbers">The collection to find the least common multiple for</param>
        /// <returns>The smallest positive long that is divisible by all longs in the provided collection</returns>
        public static long GetLeastCommonMultiple(IEnumerable<long> numbers) {
            var numberList = numbers.ToSafeList();
            if (numberList.IsEmpty()) {
                throw new EmptyCollectionException();
            }
            var max = numberList.Max();
            var currentFactor = 2L;
            var result = 1L;
            while (currentFactor < max) {
                var indices = numberList.FindIndices(i => i % currentFactor == 0);
                if (indices.Count >= 2) {
                    foreach (var index in indices) {
                        numberList[index] /= currentFactor;
                    }
                    result *= currentFactor;
                } else {
                    currentFactor++;
                }
            }
            var res = numberList.Aggregate(result, (temp, value) => temp * value);
            return res;
        }

        /// <summary>
        /// Finds the largest positive integer that divides both <see cref="a"/> and <see cref="b"/>.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>The largest positive integer that divides both <see cref="a"/> and <see cref="b"/></returns>
        public static int GetGreatestCommonDivisor(int a, int b) {
            if (a == 0) {
                return b;
            }
            return GetGreatestCommonDivisor(b % a, a);
        }

        /// <summary>
        /// Finds the largest positive long that divides both <see cref="a"/> and <see cref="b"/>.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>The largest positive integer that divides both <see cref="a"/> and <see cref="b"/></returns>
        public static long GetGreatestCommonDivisor(long a, long b) {
            if (a == 0L) {
                return b;
            }
            return GetGreatestCommonDivisor(b % a, a);
        }

        /// <summary>
        /// Finds the largest positive integer that divides each of the integers in the provided collection.
        /// </summary>
        /// <param name="numbers">The collection to find the greatest common divisor for</param>
        /// <returns>The largest positive integer that divides each of the integers in the provided collection</returns>
        public static int GetGreatestCommonDivisor(IEnumerable<int> numbers) {
            var numberList = numbers.ToSafeList();
            if (numberList.IsEmpty()) {
                throw new EmptyCollectionException();
            }
            var result = numberList[0];
            for (var i = 1; i < numberList.Count; i++) {
                result = GetGreatestCommonDivisor(numberList[i], result);
            }
            return result;
        }

        /// <summary>
        /// Finds the largest positive long that divides each of the longs in the provided collection.
        /// </summary>
        /// <param name="numbers">The collection to find the greatest common divisor for</param>
        /// <returns>The largest positive long that divides each of the longs in the provided collection</returns>
        public static long GetGreatestCommonDivisor(IEnumerable<long> numbers) {
            var numberList = numbers.ToSafeList();
            if (numberList.IsEmpty()) {
                throw new EmptyCollectionException();
            }
            var result = numberList[0];
            for (var i = 1; i < numberList.Count; i++) {
                result = GetGreatestCommonDivisor(numberList[i], result);
            }
            return result;
        }

        /// <summary>
        /// Returns whether all the provided <see cref="values"/> have the same sign.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool HaveSameSign(params decimal[] values) {
            var isNegative = values[0] < 0;
            for (var i = 1; i < values.Length; i++) {
                var current = values[i] < 0;
                if (current != isNegative) { return false; }
            }
            return true;
        }
    }
}