using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLTaxScheme {
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLIdentifier ID { get; set; }

        public UBLTaxScheme(UBLIdentifier id) {
            ID = id;
        }

        public UBLTaxScheme() {
        }
    }
}