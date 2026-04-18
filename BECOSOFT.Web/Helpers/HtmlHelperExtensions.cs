using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace BECOSOFT.Web.Helpers {
    public static class HtmlHelperExtensions {
        public static string GetURLFriendlyAlias(string input, int maxlen = 80) {
            if (input == null) return "";

            var len = input.Length;
            var prevdash = false;
            var sb = new StringBuilder(len);
            char c;

            for (var i = 0; i < len; i++) {
                c = input[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')) {
                    sb.Append(c);
                    prevdash = false;
                } else if (c >= 'A' && c <= 'Z') {
                    // convert to lowercase
                    sb.Append((char)(c | 32));
                    prevdash = false;
                } else if (c == ' ' || c == ',' || c == '.' || c == '/' ||
                           c == '\\' || c == '-' || c == '_' || c == '=') {
                    if (!prevdash && sb.Length > 0) {
                        sb.Append('-');
                        prevdash = true;
                    }
                } else if ((int)c >= 128) {
                    var prevlen = sb.Length;
                    sb.Append(RemapInternationalCharToAscii(c));
                    if (prevlen != sb.Length) prevdash = false;
                }
                if (i == maxlen) break;
            }

            if (prevdash)
                return sb.ToString().Substring(0, sb.Length - 1);
            else
                return sb.ToString();
        }

        public static string RemapInternationalCharToAscii(char c) {
            var s = c.ToString().ToLowerInvariant();
            if ("àåáâäãåą".Contains(s)) {
                return "a";
            } else if ("èéêëę".Contains(s)) {
                return "e";
            } else if ("ìíîïı".Contains(s)) {
                return "i";
            } else if ("òóôõöøőð".Contains(s)) {
                return "o";
            } else if ("ùúûüŭů".Contains(s)) {
                return "u";
            } else if ("çćčĉ".Contains(s)) {
                return "c";
            } else if ("żźž".Contains(s)) {
                return "z";
            } else if ("śşšŝ".Contains(s)) {
                return "s";
            } else if ("ñń".Contains(s)) {
                return "n";
            } else if ("ýÿ".Contains(s)) {
                return "y";
            } else if ("ğĝ".Contains(s)) {
                return "g";
            } else if (c == 'ř') {
                return "r";
            } else if (c == 'ł') {
                return "l";
            } else if (c == 'đ') {
                return "d";
            } else if (c == 'ß') {
                return "ss";
            } else if (c == 'Þ') {
                return "th";
            } else if (c == 'ĥ') {
                return "h";
            } else if (c == 'ĵ') {
                return "j";
            } else {
                return "";
            }
        }

        public static string RequireScript(this HtmlHelper html, string path, int priority = 1) {
            var requiredScripts = HttpContext.Current.Items["RequiredScripts"] as List<ResourceInclude>;
            if (requiredScripts == null) {
                HttpContext.Current.Items["RequiredScripts"] = requiredScripts = new List<ResourceInclude>();
            }

            if (requiredScripts.All(i => i.Path != path)) {
                requiredScripts.Add(new ResourceInclude { Path = path, Priority = priority });
            }
            return null;
        }

        public static HtmlString EmitRequiredScripts(this HtmlHelper html) {
            var requiredScripts = HttpContext.Current.Items["RequiredScripts"] as List<ResourceInclude>;
            if (requiredScripts == null) {
                return null;
            }

            var sb = new StringBuilder();
            foreach (var item in requiredScripts.OrderByDescending(i => i.Priority)) {
                sb.AppendFormat("<script src=\"{0}\" type=\"text/javascript\"></script>\n", item.Path);
            }
            return new HtmlString(sb.ToString());
        }

        public class ResourceInclude {
            public string Path { get; set; }
            public int Priority { get; set; }
        }
    }
}