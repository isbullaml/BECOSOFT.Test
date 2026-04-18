using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLBaseInvoiceLine {
        /// <summary>
        /// A unique identifier for the individual line within the Invoice.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLIdentifier ID { get; set; }


        /// <summary>
        /// The quantity of items (goods or services) that is charged in the Invoice line.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLQuantity InvoicedQuantity { get; set; }

        public virtual bool ShouldSerializeInvoicedQuantity() => InvoicedQuantity != null;

        /// <summary>
        /// The quantity of items (goods or services) that is charged in the Invoice line.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLQuantity CreditedQuantity { get; set; }

        public virtual bool ShouldSerializeCreditedQuantity() => CreditedQuantity != null;

        /// <summary>
        /// A textual note that gives unstructured information that is relevant to the Invoice line. 
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string Note { get; set; }

        public bool ShouldSerializeNote() => Note.HasValue();

        /// <summary>
        /// The total amount of the Invoice line. The amount is “net” without VAT, i.e. inclusive of line level
        /// allowances and charges as well as other relevant taxes. Must be rounded to maximum 2 decimals.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLAmount LineExtensionAmount { get; set; }

        /// <summary>
        /// When tax currency code is provided, two instances of the tax total must be present, but only one with tax subtotal.
        /// </summary>
        [XmlElement("TaxTotal", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public List<UBLTaxTotal> TaxTotals { get; set; }

        public bool ShouldSerializeTaxTotals() => TaxTotals.HasAny();

        [XmlElement("AllowanceCharge", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public List<UBLAllowanceCharge> AllowanceCharges { get; set; }

        public bool ShouldSerializeAllowanceCharges() => AllowanceCharges.HasAny();

        /// <summary>
        /// A group of business terms providing information about the goods and services invoiced. 
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public UBLItem Item { get; set; }
        
        /// <summary>
        /// A group of business terms providing information about the price applied for the goods and services invoiced on the Invoice line. 
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public UBLPrice Price { get; set; }
    }
}