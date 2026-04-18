using BECOSOFT.Utilities.Converters;
using System;
using System.Collections.Generic;
using System.Text;
using BECOSOFT.Utilities.Extensions.Collections;

namespace BECOSOFT.Utilities.Helpers {
    public static class CrockfordBase32Helper {
        private const string Symbols = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

        private static readonly Dictionary<char, char> CharacterMapping = new Dictionary<char, char> {
            { 'O', '0' },
            { 'I', '1' },
            { 'L', '1' },
        };

        public static string EncodeNumber(decimal s) {
            var number = s.ToDecimal();
            var output = new StringBuilder();

            while (number > 0) {
                var remainder = Math.Floor(number % 32).To<int>();
                number = Math.Floor(number / 32);
                output.Insert(0, Symbols[remainder]);
            }

            return output.ToString();
        }        
        
        public static decimal Decode(string s) {
            var output = 0m;
            foreach (var character in s) {
                var upperCharacter = char.ToUpper(character);
                var mappedCharacter = CharacterMapping.TryGetValueWithDefault(upperCharacter, upperCharacter);
                var index = Symbols.IndexOf(mappedCharacter);
                output *= 32;
                output += index;
            }
            return output;
        }
    }
}