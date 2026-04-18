using System;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Algorithms {
    public static class DistributionAlgorithm {

        /// <summary>
        /// Distributes <see cref="items"/> into separate lists, never exeeding the <see cref="item1Max"/> value for <see cref="Tuple{T1, T2}"/>.<see cref="Tuple{T1, T2}.Item1"/> and never exeeding the <see cref="item2Max"/> value for <see cref="Tuple{T1, T2}"/>.<see cref="Tuple{T1, T2}.Item2"/>.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="item1Max"></param>
        /// <param name="item2Max"></param>
        /// <returns></returns>
        public static List<List<Tuple<decimal, decimal>>> Distribute(List<Tuple<decimal, decimal>> items, decimal item1Max, decimal item2Max) {
            return Distribute(items, item1Max, item2Max, (list, selector) => list.Sum(selector), (a, b) => a + b);
        }

        /// <summary>
        /// Distributes <see cref="items"/> into separate lists, never exeeding the <see cref="item1Max"/> value for <see cref="Tuple{T1, T2}"/>.<see cref="Tuple{T1, T2}.Item1"/> and never exeeding the <see cref="item2Max"/> value for <see cref="Tuple{T1, T2}"/>.<see cref="Tuple{T1, T2}.Item2"/>.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="item1Max"></param>
        /// <param name="item2Max"></param>
        /// <returns></returns>
        public static List<List<Tuple<int, int>>> Distribute(List<Tuple<int, int>> items, int item1Max, int item2Max) {
            return Distribute(items, item1Max, item2Max, (list, selector) => list.Sum(selector), (a, b) => a + b);
        }

        /// <summary>
        /// Distributes <see cref="items"/> into separate lists, never exeeding the <see cref="item1Max"/> value for <see cref="Tuple{T1, T2}"/>.<see cref="Tuple{T1, T2}.Item1"/> and never exeeding the <see cref="item2Max"/> value for <see cref="Tuple{T1, T2}"/>.<see cref="Tuple{T1, T2}.Item2"/>.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="item1Max"></param>
        /// <param name="item2Max"></param>
        /// <returns></returns>
        public static List<List<Tuple<long, long>>> Distribute(List<Tuple<long, long>> items, long item1Max, long item2Max) {
            return Distribute(items, item1Max, item2Max, (list, selector) => list.Sum(selector), (a, b) => a + b);
        }


        /// <summary>
        /// Distributes <see cref="items"/> into separate lists, never exeeding the <see cref="max"/> value (with multiple values).
        /// </summary>
        /// <param name="items">Items to check.</param>
        /// <param name="item1Max">Maximum value when checking <see cref="Tuple{T1, T2}"/>.<see cref="Tuple{T1, T2}.Item1"/></param>
        /// <param name="item2Max">Maximum value when checking <see cref="Tuple{T1, T2}"/>.<see cref="Tuple{T1, T2}.Item2"/></param>
        /// <param name="summer">Sum function</param>
        /// <param name="adder">Addition function</param>
        /// <returns></returns>
        public static List<List<Tuple<T, T>>> Distribute<T>(List<Tuple<T, T>> items, T item1Max, T item2Max, 
                                                            Func<List<Tuple<T, T>>, Func<Tuple<T, T>, T>, T> summer, Func<T, T, T> adder)
            where T : struct, IComparable {
            var containers = new List<List<Tuple<T, T>>> { new List<Tuple<T, T>>() };
            foreach (var item in items) {
                for (var index = 0; index < containers.Count; index++) {
                    var container = containers[index];
                    var item1Value = summer(container, c => c.Item1);
                    var item2Value = summer(container, c => c.Item2);
                    var total1Value = adder(item1Value, item.Item1);
                    var total2Value = adder(item2Value, item.Item2);
                    if (IsGreaterThan(total1Value, item1Max) || IsGreaterThan(total2Value, item2Max)) {
                        if (index == containers.Count - 1) {
                            containers.Add(new List<Tuple<T, T>> { item });
                            break;
                        }
                        continue;
                    }
                    container.Add(item);
                    break;
                }
            }

            return containers;
        }

        /// <summary>
        /// Distributes <see cref="items"/> into separate lists, never exeeding the <see cref="max"/> value (with multiple values).
        /// </summary>
        /// <param name="items"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static List<List<decimal>> Distribute(List<decimal> items, decimal max) {
            return Distribute(items, max, list => list.Sum(), (a, b) => a + b);
        }

        /// <summary>
        /// Distributes <see cref="items"/> into separate lists, never exeeding the <see cref="max"/> value (with multiple values).
        /// </summary>
        /// <param name="items"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static List<List<int>> Distribute(List<int> items, int max) {
            return Distribute(items, max, list => list.Sum(), (a, b) => a + b);
        }

        /// <summary>
        /// Distributes <see cref="items"/> into separate lists, never exeeding the <see cref="max"/> value (with multiple values).
        /// </summary>
        /// <param name="items"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static List<List<long>> Distribute(List<long> items, long max) {
            return Distribute(items, max, list => list.Sum(), (a, b) => a + b);
        }

        /// <summary>
        /// Distributes <see cref="items"/> into separate lists, never exeeding the <see cref="max"/> value.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="max"></param>
        /// <param name="summer">Function to sum a list</param>
        /// <param name="adder">Function to add two values</param>
        /// <returns></returns>
        public static List<List<T>> Distribute<T>(List<T> items, T max, Func<List<T>, T> summer, Func<T, T, T> adder) 
            where T : struct, IComparable {
            var containers = new List<List<T>> { new List<T>() };
            foreach (var item in items) {
                for (var index = 0; index < containers.Count; index++) {
                    var container = containers[index];
                    var containerValue = summer(container);
                    var totalValue = adder(containerValue, item);
                    if (IsGreaterThan(totalValue, max)) {
                        if (index == containers.Count - 1) {
                            containers.Add(new List<T> { item });
                            break;
                        }
                        continue;
                    }
                    container.Add(item);
                    break;
                }
            }
            return containers;
        }


        private static bool IsGreaterThan<T>(this T value, T other) where T : IComparable {
            return value.CompareTo(other) > 0;
        }
    }
}