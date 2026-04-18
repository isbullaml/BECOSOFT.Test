using System.Globalization;
using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLAllowanceCharge {
        /// <summary>
        /// Use “true” when informing about Charges and “false” when informing about Allowances. 
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public bool ChargeIndicator { get; set; }

        /// <summary>
        /// The reason for the document level allowance or charge, expressed as a code.
        /// For allowances a subset of codelist UNCL5189 is to be used, and for charges codelist UNCL7161 applies.
        /// The Document level allowance reason code and the Document level allowance reason shall indicate the same allowance reason
        ///
        /// Allowance reason codes
        /// https://docs.peppol.eu/poacc/billing/3.0/codelist/UNCL5189/
        ///
        /// Charge reason codes=
        /// https://docs.peppol.eu/poacc/billing/3.0/codelist/UNCL7161/
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string AllowanceChargeReasonCode { get; set; }

        /// <summary>
        /// The reason for the document level allowance or charge, expressed as text.
        /// The Document level allowance reason code and the Document level allowance reason shall indicate the same allowance reason
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string AllowanceChargeReason { get; set; }

        /// <summary>
        /// The percentage that may be used, in conjunction with the document level allowance base amount, to calculate the document level allowance or charge amount. To state 20%, use value 20. 
        /// </summary>
        [XmlElement("MultiplierFactorNumeric", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string MultiplierFactorNumericString {
            get => MultiplierFactorNumeric?.ToString("F2", CultureInfo.InvariantCulture);
            set {
                decimal temp;
                if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out temp)) {
                    MultiplierFactorNumeric = temp;
                }
            }
        }

        /// <summary>
        /// The percentage that may be used, in conjunction with the document level allowance base amount, to calculate the document level allowance or charge amount. To state 20%, use value 20. 
        /// </summary>
        [XmlIgnore]
        public decimal? MultiplierFactorNumeric { get; set; }

        /// <summary>
        /// The amount of an allowance or a charge, without VAT. Must be rounded to maximum 2 decimals 
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLAmount Amount { get; set; }

        /// <summary>
        /// The base amount that may be used, in conjunction with the document level allowance or charge percentage,
        /// to calculate the document level allowance or charge amount. Must be rounded to maximum 2 decimals 
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLAmount BaseAmount { get; set; }

        public bool ShouldSerializeBaseAmount() => BaseAmount != null;

        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public UBLTaxCategory TaxCategory { get; set; }
    }
}