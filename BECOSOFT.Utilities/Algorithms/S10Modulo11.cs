using BECOSOFT.Utilities.Converters;
using System;
using System.Linq;

namespace BECOSOFT.Utilities.Algorithms {

    /// <summary>
    /// S10 Modulo 11 Algorithm
    /// <para>
    /// The UPU S10 standard defines a system for assigning 13-character identifiers to international postal items for the purpose of tracking and tracing them during shipping. 
    /// </para>
    /// https://en.wikipedia.org/wiki/S10_(UPU_standard)
    /// </summary>
    public static class S10Modulo11 {
        private static readonly int[] Weights = { 8, 6, 4, 2, 3, 5, 9, 7 };
        public static int CalculateCheckDigit(string value) {
            var temp = value.Trim();
            if (temp.Length != 8) {
                throw new ArgumentException("Invalid length");
            }
            if (!temp.All(char.IsDigit)) {
                throw new ArgumentException("Invalid input (all characters must be digits)");
            }
            var sum = temp.Select((c, i) => Weights[i] * c.ToString().To<int>()).Sum();
            sum = 11 - (sum % 11);
            if (sum == 10) {
                sum = 0;
            } if (sum == 11) {
                sum = 5;
            }
            return sum;
        }
    }
}
