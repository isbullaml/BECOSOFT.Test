using BECOSOFT.Utilities.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BECOSOFT.Utilities.Comparers {
    /// <summary>
    /// A comparer that can compare string alphanumerically
    /// </summary>
    public sealed class AlphanumericComparer : IComparer<string>, IComparer {
        private readonly Sorting _sorting;
        private StringBuilder _reusableBuilder = null;

        private enum Sorting {
            Ascending,
            Descending,
        }

        private enum ChunkType {
            Alphanumeric,
            Numeric,
        }

        private AlphanumericComparer(Sorting sorting) {
            _sorting = sorting;
            _reusableBuilder = new StringBuilder();
        }


        /// <summary>
        /// Returns an <see cref="AlphanumericComparer"/> that can compare strings in ascending order
        /// </summary>
        /// <returns></returns>
        public static AlphanumericComparer Ascending() {
            return new AlphanumericComparer(Sorting.Ascending);
        }


        /// <summary>
        /// Returns an <see cref="AlphanumericComparer"/> that can compare strings in descending order
        /// </summary>
        /// <returns></returns>
        public static AlphanumericComparer Descending() {
            return new AlphanumericComparer(Sorting.Descending);
        }

        /// <summary>
        /// Compare the provided strings in ascending order
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int CompareAscending(string x, string y) {
            var comparer = Ascending();
            return comparer.Compare(x, y);
        }

        /// <summary>
        /// Compare the provided strings in descending order
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int CompareDescending(string x, string y) {
            var comparer = Descending();
            return comparer.Compare(x, y);
        }

        public int Compare(string x, string y) {
            if (x == null && x == y) {
                return 0;
            }
            var xVal = x ?? "";
            var yVal = y ?? "";

            var xMarker = 0;
            var yMarker = 0;

            while (xMarker < xVal.Length || yMarker < yVal.Length) {
                if (xMarker >= xVal.Length) {
                    return -1;
                }
                if (yMarker >= yVal.Length) {
                    return 1;
                }
                var xCh = xVal[xMarker];
                var yCh = yVal[yMarker];

                var (xBuilderString, xIsNumeric) = GetNumericString(ref xMarker, xVal, xCh);
                var (yBuilderString, yIsNumeric) = GetNumericString(ref yMarker, yVal, yCh);
                var result = 0;
                if (xIsNumeric && yIsNumeric) {
                    var xNumericChunk = xBuilderString.To<long>();
                    var yNumericChunk = yBuilderString.To<long>();
                    if (xNumericChunk < yNumericChunk) {
                        result = -1;
                    }
                    if (xNumericChunk > yNumericChunk) {
                        result = 1;
                    }
                } else {
                    result = string.Compare(xBuilderString, yBuilderString, StringComparison.InvariantCulture);
                }
                if (result != 0) {
                    return result * (_sorting == Sorting.Ascending ? 1 : -1);
                }
            }
            return 0;
        }

        private (string Xstring, bool IsNumeric) GetNumericString(ref int marker, string val, char ch) {
            var xBuilder = _reusableBuilder.Clear();
            bool isCurrentlyBuildingDigit;
            if (marker < val.Length) {
                xBuilder.Append(ch);
                marker++;
                isCurrentlyBuildingDigit = char.IsDigit(ch);
                if (marker < val.Length) {
                    ch = val[marker];
                }
            } else {
                return (xBuilder.ToString(), false);
            }
            while ((marker < val.Length) && char.IsDigit(ch) == isCurrentlyBuildingDigit) {
                xBuilder.Append(ch);
                marker++;
                if (marker < val.Length) {
                    ch = val[marker];
                }
            }
            return (xBuilder.ToString(), isCurrentlyBuildingDigit);
        }

        public int Compare(object x, object y) {
            if (!(x is string xStr)) {
                xStr = x.To<string>();
            }
            if (!(y is string yStr)) {
                yStr = y.To<string>();
            }
            return Compare(xStr, yStr);
        }
    }
}

// Source:
// http://www.davekoelle.com/files/AlphanumComparator.cs

/*
 * The Alphanum Algorithm is an improved sorting algorithm for strings
 * containing numbers.  Instead of sorting numbers in ASCII order like
 * a standard sort, this algorithm sorts numbers in numeric order.
 *
 * The Alphanum Algorithm is discussed at http://www.DaveKoelle.com
 *
 * Based on the Java implementation of Dave Koelle's Alphanum algorithm.
 * Contributed by Jonathan Ruckwood <jonathan.ruckwood@gmail.com>
 *
 * Adapted by Dominik Hurnaus <dominik.hurnaus@gmail.com> to
 *   - correctly sort words where one word starts with another word
 *   - have slightly better performance
 *
 * Released under the MIT License - https://opensource.org/licenses/MIT
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */
