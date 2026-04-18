using System.Collections.Generic;

namespace BECOSOFT.Web.Extensions.Icons {
    public class FontAwesomeIconAttribute : IconAttribute {
        public string ClassName { get; set; }

        public FontAwesomeIconAttribute(string className) {
            ClassName = className;
        }

        public override Dictionary<string, string> GetAttributes() {
            return new Dictionary<string, string> {
                {"class", ClassName },
            };
        }
    }
}