using BECOSOFT.Utilities.Extensions;
using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLPartyLegalEntity {
        /// <summary>
        /// The full formal name by which the Seller is registered in the national registry of legal entities or as a Taxable person or otherwise trades as a person or persons. 
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string RegistrationName { get; set; }

        /// <summary>
        /// An identifier issued by an official registrar that identifies the Seller as a legal entity or person.
        /// In order for the buyer to automatically identify a supplier, the Seller identifier (BT-29), the Seller legal registration identifier (BT-30) and/or the Seller VAT identifier (BT-31) shall be present 
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLIdentifier CompanyID { get; set; }

        public bool ShouldSerializeCompanyID() => CompanyID != null;

        /// <summary>
        /// Additional legal information relevant for the Seller.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string CompanyLegalForm { get; set; }

        public bool ShouldSerializeCompanyLegalForm() => CompanyLegalForm.HasValue();
    }
}