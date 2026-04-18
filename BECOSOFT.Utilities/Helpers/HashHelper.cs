using System;
using System.Security.Cryptography;
using System.Text;

namespace BECOSOFT.Utilities.Helpers {
    public static class HashHelper {
        public static byte[] GetMd5ByteArrayFromString(string valueToHash) {
            using (var md5 = MD5.Create()) {
                var bytes = Encoding.UTF8.GetBytes(valueToHash);
                return md5.ComputeHash(bytes);
            }
        }
        public static string GetMd5StringFromString(string valueToHash) {
            var hash = GetMd5ByteArrayFromString(valueToHash);
            return Convert.ToBase64String(hash);
        }
    }
}
