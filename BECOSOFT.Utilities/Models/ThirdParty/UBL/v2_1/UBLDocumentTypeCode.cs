namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public enum UBLDocumentTypeCode {
        /// <summary>
        /// Debit information related to a transaction for goods or services to the relevant party. 
        /// </summary>
        DebitNoteRelatedToGoodsOrServices = 80,
        /// <summary>
        /// Credit information related to a transaction for goods or services to the relevant party. 
        /// </summary>
        CreditNoteRelatedToGoodsOrServices = 80,

        /// <summary>
        /// Document/message claiming payment for the supply of metered services (e.g., gas, electricity, etc.) supplied to a fixed meter whose consumption is measured over a period of time. 
        /// </summary>
        MeteredServiceInvoice = 82,

        /// <summary>
        /// Document message for providing credit information related to financial adjustments to the relevant party, e.g., bonuses. 
        /// </summary>
        CreditNoteRelatedToFinancialAdjustments = 83,

        /// <summary>
        /// Document/message for providing debit information related to financial adjustments to the relevant party. 
        /// </summary>
        DebitNoteRelatedToFinancialAdjustments = 84,

        /// <summary>
        /// (1334) Document/message claiming payment for goods or services supplied under conditions agreed between seller and buyer. 
        /// </summary>
        CommercialInvoice = 380,

        /// <summary>
        /// Document/message for providing credit information to the relevant party.
        /// </summary>
        CreditNote = 381,

        /// <summary>
        /// Document/message for providing debit information to the relevant party. 
        /// </summary>
        DebitNote = 383,

        /// <summary>
        /// An invoice to pay amounts for goods and services in advance; these amounts will be subtracted from the final invoice. 
        /// </summary>
        PrepaymentInvoice = 386,

        /// <summary>
        /// Invoice assigned to a third party for collection.
        /// </summary>
        FactoredInvoice = 393,

        /// <summary>
        /// Commercial invoice that covers a transaction other than one involving a sale. 
        /// </summary>
        ConsignmentInvoice = 395,

        /// <summary>
        /// Credit note related to assigned invoice(s).
        /// </summary>
        FactoredCreditNote = 396,

        /// <summary>
        /// Document/message for providing credit information to the relevant party. 
        /// </summary>
        ForwardersCreditNote = 532,

        /// <summary>
        /// Document/message issued by an insurer specifying the cost of an insurance which has been effected and claiming payment therefore. 
        /// </summary>
        InsurersInvoice = 575,

        /// <summary>
        /// Invoice issued by a freight forwarder specifying services rendered and costs incurred and claiming payment therefore. 
        /// </summary>
        ForwardersInvoice = 623,

        /// <summary>
        /// Document/message issued by a transport operation specifying freight costs and charges incurred for a transport operation and stating conditions of payment. 
        /// </summary>
        FreightInvoice = 780,
    }
}