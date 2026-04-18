using System.Collections.Generic;

namespace BECOSOFT.Web.Extensions.Icons {
    public class FontAwesomeSizeAttribute : IconSizeAttribute {
        public string ClassName { get; set; }

        public FontAwesomeSizeAttribute(string className) {
            ClassName = className;
        }

        public override Dictionary<string, string> GetAttributes() {
            return new Dictionary<string, string> {
                {"class", ClassName },
            };
        }
    }
}