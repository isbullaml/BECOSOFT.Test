using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLTaxSubtotal {
        /// <summary>
        /// Sum of all taxable amounts subject to a specific VAT category code and VAT category rate (if the VAT category rate is applicable).
        /// Must be rounded to maximum 2 decimals. 
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLAmount TaxableAmount { get; set; }

        public bool ShouldSerializeTaxableAmount() => TaxableAmount != null;

        /// <summary>
        /// The total VAT amount for a given VAT category. Must be rounded to maximum 2 decimals.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLAmount TaxAmount { get; set; }

        /// <summary>
        /// The amount of this tax subtotal, expressed in the currency used for invoicing.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLAmount TransactionCurrencyTaxAmount { get; set; }

        public bool ShouldSerializeTransactionCurrencyTaxAmount() => TransactionCurrencyTaxAmount != null;

        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public UBLTaxCategory TaxCategory { get; set; }
    }
}