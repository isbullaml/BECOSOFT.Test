using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public abstract class UBLBaseInvoiceDocument : UBLDocument {
        [XmlElement("DueDate", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string DueDateString {
            get => DueDate.ToString("yyyy-MM-dd");
            set => DueDate = DateTimeHelpers.Parse(value);
        }

        /// <summary>
        /// The date when the payment is due.Format "YYYY-MM-DD". In case the Amount due for payment (BT-115) is positive,
        /// either the Payment due date (BT-9) or the Payment terms (BT-20) shall be present.
        /// </summary>
        [XmlIgnore]
        public DateTime DueDate { get; set; }

        [XmlElement("InvoiceTypeCode", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string InvoiceTypeCodeString {
            get => DocumentTypeCode.ToInt().ToString();
            set => DocumentTypeCode = value.To<UBLDocumentTypeCode>();
        }
        
        [XmlIgnore]
        public virtual UBLDocumentTypeCode DocumentTypeCode { get; set; } = UBLDocumentTypeCode.CommercialInvoice;

        /// <summary>
        /// A textual note that gives unstructured information that is relevant to the Invoice as a whole.
        /// Such as the reason for any correction or assignment note in case the invoice has been factored. 
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string Note { get; set; }

        [XmlElement("TaxPointDate", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string TaxPointDateString {
            get => TaxPointDate?.ToString("yyyy-MM-dd");
            set => TaxPointDate = DateTimeHelpers.Parse(value);
        }

        public bool ShouldSerializeTaxPointDateString() => TaxPointDate.HasValue;

        /// <summary>
        /// The date when the VAT becomes accountable for the Seller
        /// and for the Buyer in so far as that date can be determined
        /// and differs from the date of issue of the invoice, according to the VAT directive.
        /// This element is required if the Value added tax point date is different from the Invoice issue date. 
        /// </summary>
        [XmlIgnore]
        public DateTime? TaxPointDate { get; set; }

        /// <summary>
        /// The currency in which all Invoice amounts are given, except for the Total VAT amount in accounting currency.
        /// Only one currency shall be used in the Invoice, except for the VAT accounting currency code (BT-6) and the invoice total VAT amount in accounting currency (BT-111). 
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string DocumentCurrencyCode { get; set; }

        /// <summary>
        /// The currency used for VAT accounting and reporting purposes as accepted or required in the country of the Seller.
        /// Shall be used in combination with the Invoice total VAT amount in accounting currency (BT-111), when the VAT accounting currency code differs from the Invoice currency code. 
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string TaxCurrencyCode { get; set; }

        public bool ShouldSerializeTaxCurrencyCode() => TaxCurrencyCode.HasValue();

        /// <summary>
        /// An identifier assigned by the Buyer used for internal routing purposes. An invoice must have buyer reference or purchase order reference (BT-13).
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string BuyerReference { get; set; }

        public bool ShouldSerializeBuyerReference() => BuyerReference.HasValue();

        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public UBLOrderReference OrderReference { get; set; }

        public bool ShouldSerializeOrderReference() => BuyerReference.IsNullOrEmpty();

        /// <summary>
        /// A group of business terms providing information about additional supporting documents substantiating the claims made in the Invoice.
        /// The additional supporting documents can be used for both referencing a document number which is expected to be known by the receiver,
        /// an external document (referenced by a URL) or as an embedded document, Base64 encoded (such as a time report).
        /// </summary>
        [XmlElement("AdditionalDocumentReference", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public List<UBLAdditionalDocumentReference> AdditionalDocumentReferences { get; set; }

        public bool ShouldSerializeAdditionalDocumentReferences() => AdditionalDocumentReferences.HasAny();

        /// <summary>
        /// A group of business terms providing information about the Seller.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public UBLAccountingParty AccountingSupplierParty { get; set; }

        /// <summary>
        /// A group of business terms providing information about the Buyer.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public UBLAccountingParty AccountingCustomerParty { get; set; }

        /// <summary>
        /// A group of business terms providing information about the Payee, i.e. the role that receives the payment.
        /// Shall be used when the Payee is different from the Seller. 
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public UBLParty PayeeParty { get; set; }

        public bool ShouldSerializePayeeParty() => PayeeParty != null;

        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public UBLDelivery Delivery { get; set; }

        public bool ShouldSerializeDelivery() => Delivery != null;

        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public List<UBLPaymentMeans> PaymentMeans { get; set; }

        public bool ShouldSerializePaymentMeans() => PaymentMeans.HasAny();

        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public UBLPaymentTerms PaymentTerms { get; set; }

        public bool ShouldSerializePaymentTerms() => PaymentTerms != null;

        [XmlElement("AllowanceCharge", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public List<UBLAllowanceCharge> AllowanceCharges { get; set; }

        public bool ShouldSerializeAllowanceCharges() => AllowanceCharges.HasAny();

        /// <summary>
        /// When tax currency code is provided, two instances of the tax total must be present, but only one with tax subtotal.
        /// </summary>
        [XmlElement("TaxTotal", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public List<UBLTaxTotal> TaxTotals { get; set; }

        public bool ShouldSerializeTaxTotals() => TaxTotals.HasAny();

        /// <summary>
        /// Document totals
        /// A group of business terms providing the monetary totals for the Invoice.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public UBLLegalMonetaryTotal LegalMonetaryTotal { get; set; }
    }
}