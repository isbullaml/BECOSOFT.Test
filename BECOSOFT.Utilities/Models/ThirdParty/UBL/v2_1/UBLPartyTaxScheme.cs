using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLPartyTaxScheme {
        /// <summary>
        /// The Seller's VAT identifier (also known as Seller VAT identification number)
        /// or the local identification (defined by the Seller’s address) of the Seller for tax purposes
        /// or a reference that enables the Seller to state his registered tax status.
        /// In order for the buyer to automatically identify a supplier, the Seller identifier (BT-29), the Seller
        /// legal registration identifier (BT-30) and/or the Seller VAT identifier (BT-31) shall be present
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLIdentifier CompanyID { get; set; }

        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public UBLTaxScheme TaxScheme { get; set; }
    }
}