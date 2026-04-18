using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLLegalMonetaryTotal {
        /// <summary>
        /// Sum of all Invoice line net amounts in the Invoice. Must be rounded to maximum 2 decimals.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLAmount LineExtensionAmount { get; set; }

        /// <summary>
        /// The total amount of the Invoice without VAT. Must be rounded to maximum 2 decimals.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLAmount TaxExclusiveAmount { get; set; }

        /// <summary>
        /// The total amount of the Invoice with VAT. Must be rounded to maximum 2 decimals.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLAmount TaxInclusiveAmount { get; set; }

        /// <summary>
        /// Sum of all allowances on document level in the Invoice. Must be rounded to maximum 2 decimals.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLAmount AllowanceTotalAmount { get; set; }

        public bool ShouldSerializeAllowanceTotalAmount() => AllowanceTotalAmount != null;

        /// <summary>
        /// Sum of all charges on document level in the Invoice. Must be rounded to maximum 2 decimals.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLAmount ChargeTotalAmount { get; set; }

        public bool ShouldSerializeChargeTotalAmount() => ChargeTotalAmount != null;

        /// <summary>
        /// The sum of amounts which have been paid in advance. Must be rounded to maximum 2 decimals.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLAmount PrepaidAmount { get; set; }

        public bool ShouldSerializePrepaidAmount() => PrepaidAmount != null;

        /// <summary>
        /// The amount to be added to the invoice total to round the amount to be paid. Must be rounded to maximum 2 decimals.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLAmount PayableRoundingAmount { get; set; }

        public bool ShouldSerializePayableRoundingAmount() => PayableRoundingAmount != null;

        /// <summary>
        /// The outstanding amount that is requested to be paid. Must be rounded to maximum 2 decimals.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLAmount PayableAmount { get; set; }
    }
}