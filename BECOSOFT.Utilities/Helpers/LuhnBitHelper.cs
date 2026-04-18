using System.Linq;

namespace BECOSOFT.Utilities.Helpers {
    public static class LuhnBitHelper {
        public static int GetLuhnBit(string s) {
            var reversed = s.Reverse();
            var doubleDigit = true;
            var sum = 0;
            foreach (var c in reversed) {
                var integer = c - '0';
                
                if (doubleDigit) {
                    integer *= 2;
                    sum += SumDigits(integer);
                } else {
                    sum += integer;
                }
                doubleDigit = !doubleDigit;
            }

            return (10 - (sum % 10)) % 10;
        }

        private static int SumDigits(int number) {
            var output = 0;
            while (number != 0) {
                output += number % 10;
                number /= 10;
            }
            return output;
        }
    }
}