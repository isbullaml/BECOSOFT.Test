using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLPaymentMeans {
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLPaymentMeansCode PaymentMeansCode { get; set; }

        /// <summary>
        /// A textual value used to establish a link between the payment and the Invoice, issued by the Seller.
        /// Used for creditor's critical reconciliation information.
        /// This information element helps the Seller to assign an incoming payment to the relevant payment process. 
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string PaymentID { get; set; }

        /// <summary>
        /// A group of business terms to specify credit transfer payments.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public UBLPayeeFinancialAccount PayeeFinancialAccount { get; set; }
    }
}