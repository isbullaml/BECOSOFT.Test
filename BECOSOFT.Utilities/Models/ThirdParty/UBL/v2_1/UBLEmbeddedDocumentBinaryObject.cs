using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLEmbeddedDocumentBinaryObject {
        [XmlText]
        public string Base64Value { get; set; }

        /// <summary>
        /// Mime code list: https://docs.peppol.eu/poacc/billing/3.0/codelist/MimeCode/
        /// </summary>
        [XmlAttribute("mimeCode")]
        public string MimeType { get; set; }

        [XmlAttribute("filenSame")]
        public string FileName { get; set; }
    }
}