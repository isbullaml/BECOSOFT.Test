namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLInvoiceLine : UBLBaseInvoiceLine {

        public override bool ShouldSerializeInvoicedQuantity() => true;

    }
}