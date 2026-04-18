using BECOSOFT.Utilities.Extensions;
using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLAdditionalDocumentReference {
        /// <summary>
        /// An identifier for an object on which the invoice is based, given by the Seller, or the identifier for the supporting document.
        ///
        /// https://docs.peppol.eu/poacc/billing/3.0/codelist/UNCL1153/
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLIdentifier ID { get; set; }

        /// <summary>
        /// Code "130" MUST be used to indicate an invoice object reference. Not used for other additional documents
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string DocumentTypeCode { get; set; }

        public bool ShouldSerializeDocumentTypeCode() => DocumentTypeCode.HasValue();

        /// <summary>
        /// A description of the supporting document, such as: timesheet, usage report etc.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string DocumentDescription { get; set; }

        public bool ShouldSerializeDocumentDescription() => DocumentDescription.HasValue();

        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLAttachment Attachment { get; set; }

        public bool ShouldSerializeAttachment() => Attachment != null;
    }
}