using System;
using System.IO;
using System.Security.Cryptography;

namespace BECOSOFT.Utilities.Security {
    /// <summary>
    /// This class contains methods to create MD5 hashes.
    /// </summary>
    public static class Md5HashCreator {
        /// <summary>
        /// Create an MD5 hash from the contents of a <paramref name="file"/>
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string CreateHash(FileInfo file) {
            using (var md5 = MD5.Create()) {
                using (var stream = file.OpenRead()) {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        /// <summary>
        /// Create an MD5 hash from a <see cref="byte"/>-array
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string CreateHash(byte[] bytes) {
            using (var md5 = MD5.Create()) {
                var hash = md5.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}