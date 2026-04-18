using BECOSOFT.Utilities.Extensions;
using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLContact {
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string Name { get; set; }

        public bool ShouldSerializeName() => Name.HasValue();

        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string Telephone { get; set; }

        public bool ShouldSerializeTelephone() => Telephone.HasValue();

        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string ElectronicMail { get; set; }

        public bool ShouldSerializeElectronicMail() => ElectronicMail.HasValue();
    }
}