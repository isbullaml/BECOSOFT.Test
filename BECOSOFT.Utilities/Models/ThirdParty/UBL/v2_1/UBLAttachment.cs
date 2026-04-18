using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLAttachment {
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLEmbeddedDocumentBinaryObject EmbeddedDocumentBinaryObject { get; set; }

        public bool ShouldSerializeEmbeddedDocumentBinaryObject() => EmbeddedDocumentBinaryObject != null;

        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLExternalReference ExternalReference { get; set; }

        public bool ShouldSerializeExternalReference() => ExternalReference != null;
    }
}