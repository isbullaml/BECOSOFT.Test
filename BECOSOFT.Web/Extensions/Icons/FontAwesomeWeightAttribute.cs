using System.Collections.Generic;

namespace BECOSOFT.Web.Extensions.Icons {
    public class FontAwesomeWeightAttribute : IconWeightAttribute {
        public string ClassName { get; set; }

        public FontAwesomeWeightAttribute(string className) {
            ClassName = className;
        }

        public override Dictionary<string, string> GetAttributes() {
            return new Dictionary<string, string> {
                {"class", ClassName },
            };
        }
    }
}