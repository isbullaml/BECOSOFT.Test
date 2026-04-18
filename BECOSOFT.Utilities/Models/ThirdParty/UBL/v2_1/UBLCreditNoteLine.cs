namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLCreditNoteLine : UBLBaseInvoiceLine {

        public override bool ShouldSerializeCreditedQuantity() => true;

    }
}