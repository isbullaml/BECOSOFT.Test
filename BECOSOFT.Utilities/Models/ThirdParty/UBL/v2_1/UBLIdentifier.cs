using BECOSOFT.Utilities.Extensions;
using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLIdentifier {
        [XmlText]
        public string Value { get; set; }

        /// <summary>
        /// https://docs.peppol.eu/poacc/billing/3.0/codelist/ICD/
        /// </summary>
        [XmlAttribute("schemeID")]
        public string SchemeID { get; set; }

        public virtual bool ShouldSerializeSchemeID() => SchemeID.HasValue();

        [XmlAttribute("schemeAgencyID")]
        public string SchemeAgencyID { get; set; }

        public virtual bool ShouldSerializeSchemeAgencyID() => SchemeAgencyID.HasValue();

        public UBLIdentifier() {
        }

        public UBLIdentifier(string value) : this(value, null, null) {
        }

        public UBLIdentifier(string value, string schemeID) : this(value, schemeID, null) {
        }

        public UBLIdentifier(string value, string schemeID, string schemeAgencyID) {
            Value = value;
            SchemeID = schemeID;
            SchemeAgencyID = schemeAgencyID;
        }
    }
}