using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BECOSOFT.Utilities.Security {
    /// <summary>
    /// Class that provides all random passwords and encryption
    /// Source: <see href="http://stackoverflow.com/a/2791259"/>
    /// </summary>
    public static class Encryptor {
        /// <summary>
        /// The key used for encrypting
        /// </summary>
        private const string CryptKey = "bpvx8mKCLiawPege85kAC1EvJczIrqoF5Vqhm8Ge";

        /// <summary>
        /// The amount of iterations used when hashing
        /// </summary>
        private const int IterationCount = 10000;

        /// <summary>
        /// The size of the key used for encryption
        /// </summary>
        private const int KeySize = 32;

        /// <summary>
        /// The blocksize used for encryption
        /// </summary>
        private const int BlockSize = 256;

        /// <summary>
        /// List of possible characters
        /// </summary>
        private static readonly char[] Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

        /// <summary>
        /// Decrypts a string with a password
        /// </summary>
        /// <param name="cipherText">The encrypted string</param>
        /// <param name="cryptKey">The key used for encrypting</param>
        /// <returns>The decrypted strings</returns>
        public static string Decrypt(string cipherText, string cryptKey) {
            var passPhrase = ConvertToBase64String(CryptKey + cryptKey);
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(KeySize).ToArray();
            var initializationVectorStringBytes = cipherTextBytesWithSaltAndIv.Skip(KeySize).Take(KeySize).ToArray();
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip(KeySize * 2).Take(cipherTextBytesWithSaltAndIv.Length - KeySize * 2).ToArray();
            string result;

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, IterationCount)) {
                var keyBytes = password.GetBytes(KeySize);
                using (var symmetricKey = GetSymmetricKey()) {
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, initializationVectorStringBytes)) {
                        using (var memoryStream = new MemoryStream(cipherTextBytes)) {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read)) {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                result = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Encrypts a string with a password
        /// </summary>
        /// <param name="plainText">The string</param>
        /// <param name="cryptKey">The key used for encrypting</param>
        /// <returns>The encrypted string</returns>
        public static string Encrypt(string plainText, string cryptKey) {
            var passPhrase = ConvertToBase64String(CryptKey + cryptKey);
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var initializationVectorStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            string result;

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, IterationCount)) {
                var keyBytes = password.GetBytes(KeySize);
                using (var symmetricKey = GetSymmetricKey()) {
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, initializationVectorStringBytes)) {
                        using (var memoryStream = new MemoryStream()) {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write)) {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(initializationVectorStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                result = Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Generates a random alphanumerical string
        /// </summary>
        /// <param name="maxSize">The maximum size of the string. Default is 40.</param>
        /// <param name="chars">The characters to use in the random string. Default is alphanumeric.</param>
        /// <returns>A random string</returns>
        public static string GetUniqueKey(int maxSize = 40, char[] chars = null) {
            var result = new StringBuilder(maxSize);
            var charsToUse = chars ?? Chars;
            var isValid = false;
            while (!isValid) {
                result = new StringBuilder(maxSize);
                var data = new byte[1];
                using (var crypto = new RNGCryptoServiceProvider()) {
                    crypto.GetNonZeroBytes(data);
                    data = new byte[maxSize];
                    crypto.GetNonZeroBytes(data);
                }

                foreach (var b in data) {
                    result.Append(charsToUse[b % charsToUse.Length]);
                }

                var resultString = result.ToString();
                if (resultString.Any(c => char.IsUpper(c) || char.IsLower(c) || char.IsDigit(c))) {
                    isValid = true;
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Encodes a UTF8 string to a Base64 string
        /// </summary>
        /// <param name="str">The UTF8 string</param>
        /// <returns>The Base64 string</returns>
        private static string ConvertToBase64String(string str) {
            var bytes = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Generates 256 random bits
        /// </summary>
        /// <returns>The 256 random bits as a byte-array</returns>
        private static byte[] Generate256BitsOfRandomEntropy() {
            var randomBytes = new byte[32];
            using (var rngCsp = new RNGCryptoServiceProvider()) {
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }

        /// <summary>
        /// Gets a <see cref="RijndaelManaged"/> object, initialized with parameters
        /// </summary>
        /// <returns></returns>
        private static RijndaelManaged GetSymmetricKey() {
            return new RijndaelManaged {
                BlockSize = BlockSize,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };
        }
    }
}