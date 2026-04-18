using System.Collections.Generic;
using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    [XmlRoot("CreditNote", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2")]
    public class UBLCreditNote : UBLBaseInvoiceDocument {
        [XmlIgnore]
        public override UBLDocumentTypeCode DocumentTypeCode { get; set; } = UBLDocumentTypeCode.CreditNote;

        /// <summary>
        /// A group of business terms providing information on individual Invoice lines.
        /// </summary>
        [XmlElement("CreditNoteLine", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public List<UBLCreditNoteLine> CreditNoteLines { get; set; }
    }
    [XmlRoot("Invoice", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2")]
    public class UBLInvoice : UBLBaseInvoiceDocument {
        [XmlIgnore]
        public override UBLDocumentTypeCode DocumentTypeCode { get; set; } = UBLDocumentTypeCode.CommercialInvoice;

        /// <summary>
        /// A group of business terms providing information on individual Invoice lines.
        /// </summary>
        [XmlElement("InvoiceLine", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public List<UBLInvoiceLine> InvoiceLines { get; set; }
    }
}