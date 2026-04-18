using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLPriceAllowanceCharge {
        /// <summary>
        /// Use “true” when informing about Charges and “false” when informing about Allowances. 
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public bool ChargeIndicator { get; set; }

        /// <summary>
        /// The total discount subtracted from the Item gross price to calculate the Item net price. 
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLAmount Amount { get; set; }

        /// <summary>
        /// The unit price, exclusive of VAT, before subtracting Item price discount, can not be negative
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLAmount BaseAmount { get; set; }

        public bool ShouldSerializeBaseAmount() => BaseAmount != null;
    }
}