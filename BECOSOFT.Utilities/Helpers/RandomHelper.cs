using BECOSOFT.Utilities.Converters;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BECOSOFT.Utilities.Helpers {
    /// <summary>
    /// Class containing various methods to generate random strings, numbers, ...
    /// </summary>
    public static class RandomHelper {
        private static readonly char[] _alphabeticCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static readonly char[] _numericCharacters = "1234567890".ToCharArray();
        private static readonly char[] _alphanumericCharacters = _alphabeticCharacters.Union(_numericCharacters).ToArray();

        /// <summary>
        /// Generates a random integer of the specified <see cref="length"/>.
        /// The generated number never starts with 0.
        /// Max length is 9.
        /// </summary>
        /// <param name="length">Max length is 9 </param>
        /// <returns></returns>
        public static int GetRandomInteger(int length) {
            if (length <= 0 || length > 9) {
                throw new ArgumentException("Length must be between 1 and 9", nameof(length));
            }
            return GetRandomNumber<int>(length);
        }

        /// <summary>
        /// Generates a random integer of the specified <see cref="length"/>.
        /// The generated number never starts with 0.
        /// Max length is 18.
        /// </summary>
        /// <param name="length">Max length is 18 </param>
        /// <returns></returns>
        public static long GetRandomLong(int length) {
            if (length <= 0 || length > 18) {
                throw new ArgumentException("Length must be between 1 and 18", nameof(length));
            }
            return GetRandomNumber<long>(length);
        }

        private static T GetRandomNumber<T>(int length) {
            // since a integral number type cannot start with 0
            // we offset the char array to exclude 0 from available characters for the first index
            var generatedString = InternalGetRandomString(length, RandomType.Numeric, offsetFirst: -1);
            return generatedString.To<T>();
        }
        
        /// <summary>
        /// Generates a random string of the specified <see cref="length"/> and <see cref="randomType"/>
        /// </summary>
        /// <param name="length"></param>
        /// <param name="randomType">Default: <see cref="RandomType.Alphabetic"/></param>
        /// <returns></returns>
        public static string GetRandomString(int length, RandomType randomType = RandomType.Alphabetic) {
            return InternalGetRandomString(length, randomType);
        }

        private static string InternalGetRandomString(int length, RandomType randomType, int offsetFirst = 0) {
            var chars = GetCharArray(randomType);
            using (var rand = new RNGCryptoServiceProvider()) {
                var s = new StringBuilder();
                var intBytes = new byte[4 * length];
                rand.GetBytes(intBytes);
                for (var i = 0; i < length; i++) {
                    var randomInt = BitConverter.ToUInt32(intBytes, i * 4);
                    var modValue = chars.Length + (i == 0 ? offsetFirst : 0);
                    var index = randomInt % modValue;
                    s.Append(chars[index]);
                }
                return s.ToString();
            }
        }

        private static char[] GetCharArray(RandomType type) {
            switch (type) {
                case RandomType.Alphanumeric:
                    return _alphanumericCharacters;
                case RandomType.Numeric:
                    return _numericCharacters;
                case RandomType.Alphabetic:
                default:
                    return _alphabeticCharacters;
            }
        }
    }

    public enum RandomType {
        Alphabetic = 0,
        Alphanumeric = 1,
        Numeric = 2,
    }
}
