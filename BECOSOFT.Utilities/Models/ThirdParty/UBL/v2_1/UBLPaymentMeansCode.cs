using BECOSOFT.Utilities.Extensions;
using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLPaymentMeansCode {
        [XmlText]
        public string Value { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        public bool ShouldSerializeName() => Name.HasValue();

        public UBLPaymentMeansCode() {
        }

        /// <summary>
        /// https://docs.peppol.eu/poacc/billing/3.0/codelist/UNCL4461/
        /// </summary>
        /// <param name="value"></param>
        public UBLPaymentMeansCode(string value) : this(value, null) {
        }

        public UBLPaymentMeansCode(string value, string name) {
            Value = value;
            Name = name;
        }
    }
}