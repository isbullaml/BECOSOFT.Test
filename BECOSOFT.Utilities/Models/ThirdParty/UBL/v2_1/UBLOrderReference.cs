using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLOrderReference {
        /// <summary>
        /// An identifier of a referenced purchase order, issued by the Buyer.An invoice must have buyer reference (BT-10) or purchase order reference.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLIdentifier ID { get; set; }
    }
}