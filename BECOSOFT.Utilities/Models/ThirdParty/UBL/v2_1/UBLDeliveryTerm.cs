using BECOSOFT.Utilities.Extensions.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLDeliveryTerm {
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLIdentifier ID { get; set; }

        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public List<string> SpecialTerms { get; set; }

        public bool ShouldSerializeSpecialTerms() => SpecialTerms.HasAny();
    }
}