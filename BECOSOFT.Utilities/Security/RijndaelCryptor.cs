using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BECOSOFT.Utilities.Security {
    /// <summary>
    /// Legacy Encrypt and Decrypt functions (are used in BECO CRM).
    /// </summary>
    public static class RijndaelCryptor {
        private static readonly byte[] IvBytes = { 121, 241, 10, 1, 132, 74, 11, 39, 255, 91, 45, 78, 14, 211, 22, 62 };

        /// <summary>
        /// Legacy way to encrypt a string using <see cref="Rijndael"/>.
        /// </summary>
        /// <param name="toEncrypt">String to encrypt.</param>
        /// <param name="key">Encryption key</param>
        /// <returns>The encrypted string</returns>
        public static string Encrypt(string toEncrypt, string key) {
            var bytesToEncrypt = Encoding.ASCII.GetBytes(toEncrypt.Replace("\0", "").ToCharArray());
            key = GetPaddedKey(key);
            var bytesKey = Encoding.ASCII.GetBytes(key.ToCharArray());
            using (var memoryStream = new MemoryStream()) {
                var rijndael = new RijndaelManaged();
                using (var cryptoStream = new CryptoStream(memoryStream, rijndael.CreateEncryptor(bytesKey, IvBytes),
                                                           CryptoStreamMode.Write)) {
                    cryptoStream.Write(bytesToEncrypt, 0, bytesToEncrypt.Length);
                    cryptoStream.FlushFinalBlock();
                    return Convert.ToBase64String(memoryStream.ToArray());
                }
            }
        }

        /// <summary>
        /// Legacy way to decrypt an encrypted string using <see cref="Rijndael"/>.
        /// </summary>
        /// <param name="toDecrypt">String to decrypt.</param>
        /// <param name="key">Encryption key</param>
        /// <returns>The decrypted string</returns>
        public static string Decrypt(string toDecrypt, string key) {
            var bytesToDecrypt = Convert.FromBase64String(toDecrypt);
            key = GetPaddedKey(key);
            var bytesKey = Encoding.ASCII.GetBytes(key.ToCharArray());
            var temp = new byte[bytesToDecrypt.Length + 1];
            using (var memoryStream = new MemoryStream(bytesToDecrypt)) {
                var rijndael = new RijndaelManaged();
                using (var cryptoStream = new CryptoStream(memoryStream, rijndael.CreateDecryptor(bytesKey, IvBytes),
                                                           CryptoStreamMode.Read)) {
                    cryptoStream.Read(temp, 0, temp.Length);
                    return Encoding.ASCII.GetString(temp).Replace("\0", "");
                }
            }
        }

        private static string GetPaddedKey(string key) {
            const int maxKeyLength = 32;
            var keyLength = key.Length;
            if (keyLength >= maxKeyLength) {
                key = key.Substring(0, maxKeyLength);
            } else {
                key += new string('X', maxKeyLength - keyLength);
            }
            return key;
        }
    }
}