namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLEndpoint : UBLIdentifier {
        public UBLEndpoint() {
        }

        public UBLEndpoint(string value, string schemeID) : base(value, schemeID) {
        }

        public override bool ShouldSerializeSchemeID() => true;
    }
}