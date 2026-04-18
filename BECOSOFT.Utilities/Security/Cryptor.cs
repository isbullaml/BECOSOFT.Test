using BECOSOFT.Utilities.Extensions;
using Scrypt;
using System;

namespace BECOSOFT.Utilities.Security {
    /// <summary>
    /// Wraps the Scrypt encoder
    /// </summary>
    public static class Cryptor {
        private const int IterationPower = 16;
        private const int BlockSize = 8;
        private const int NumberOfThreads = 1;

        private static ScryptEncoder GetEncoder() {
            var iterationCount = (int) Math.Pow(2, IterationPower);
            return new ScryptEncoder(iterationCount, BlockSize, NumberOfThreads);
        }

        /// <summary>
        /// Compares a password with a hashed password
        /// </summary>
        /// <param name="password">The clear-text password</param>
        /// <param name="hashedPassword">The hashed password</param>
        /// <returns>Boolean indicating whether the passwords are equal</returns>
        public static bool AreEqual(string password, string hashedPassword) {
            if (password.IsNullOrWhiteSpace() || hashedPassword.IsNullOrWhiteSpace()) { return false; }
            return GetEncoder().Compare(password, hashedPassword);
        }

        /// <summary>
        /// Encodes a password
        /// </summary>
        /// <param name="password">The clear-text password</param>
        /// <returns>The hashed password</returns>
        public static string Encode(string password) {
            if (password.IsNullOrWhiteSpace()) { return ""; }
            return GetEncoder().Encode(password);
        }
    }
}
