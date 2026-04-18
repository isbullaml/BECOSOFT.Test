using BECOSOFT.Utilities.Extensions.Collections;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace BECOSOFT.Utilities.Extensions {
    public static class SHA1CryptoServiceProviderExtensions {
        /// <summary>
        /// Calculates a SHA1-hash for the given <see cref="image"/>. The hash is formatted as a string.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        public static string GetHashString(this SHA1CryptoServiceProvider provider, Image image) {
            using (var mem = new MemoryStream()) {
                image.Save(mem, image.RawFormat);
                mem.Position = 0;
                return provider.GetHashString(mem);
            }
        }

        /// <summary>
        /// Calculates a SHA1-hash for the given byte array. The hash is formatted as a string.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string GetHashString(this SHA1CryptoServiceProvider provider, byte[] data) {
            if (data.IsEmpty()) {
                return null;
            }
            var hash = provider.ComputeHash(data);
            return string.Concat(hash.Select(x => x.ToString("X2")));
        }

        /// <summary>
        /// Calculates a SHA1-hash for the given file located at <see cref="filePath"/>. The hash is formatted as a string.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">The file specified in <see cref="filePath"/> could not be found.</exception>
        public static string GetHashString(this SHA1CryptoServiceProvider provider, string filePath) {
            if (!File.Exists(filePath)) {
                throw new FileNotFoundException();
            }
            using (var reader = File.OpenRead(filePath)) {
                return provider.GetHashString(reader);
            }
        }

        /// <summary>
        /// Calculates a SHA1-hash for the given <see cref="stream"/>. The hash is formatted as a string.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string GetHashString(this SHA1CryptoServiceProvider provider, Stream stream) {
            var hash = provider.ComputeHash(stream);
            return string.Concat(hash.Select(x => x.ToString("X2")));
        }
    }
}
