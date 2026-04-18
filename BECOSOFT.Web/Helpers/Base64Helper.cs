using BECOSOFT.Utilities.Extensions;
using System.Linq;
using System.Text.RegularExpressions;

namespace BECOSOFT.Web.Helpers {
    public static class Base64Helper {
        private static readonly string MimeRegex = @"data:([a-zA-Z0-9]+\/[a-zA-Z0-9-.+]+).*,.*";

        public static string GetMimeType(string base64) {
            var regex = new Regex(MimeRegex);
            var parts = regex.Split(base64).Where(r => !r.IsNullOrWhiteSpace());
            return parts.FirstOrDefault();
        }

        public static string GetFilePart(string base64) {
            var imgBase64 = base64.ToSplitList<string>("base64,");
            return imgBase64.Count > 1 ? imgBase64[1] : base64;
        }
    }
}