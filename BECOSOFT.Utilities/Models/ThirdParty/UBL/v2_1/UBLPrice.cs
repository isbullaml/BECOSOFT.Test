using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    /// <summary>
    /// A group of business terms providing information about the price applied for the goods and services invoiced on the Invoice line. 
    /// </summary>
    public class UBLPrice {
        /// <summary>
        /// The price of an item, exclusive of VAT, after subtracting item price discount. The Item net price has to be equal with the Item gross price less the Item price discount, if they are both provided. Item price can not be negative.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLAmount PriceAmount { get; set; }

        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLQuantity BaseQuantity { get; set; }

        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public UBLPriceAllowanceCharge AllowanceCharge { get; set; }
    }
}