using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLItemClassificationCode {
        /// <summary>
        /// A code for classifying the item by its type or nature.
        /// </summary>
        [XmlText]
        public string Value { get; set; }
        /// <summary>
        /// The identification scheme identifier of the Item classification identifier
        ///
        /// https://docs.peppol.eu/poacc/billing/3.0/codelist/UNCL5305/
        /// </summary>
        [XmlAttribute("listID")]
        public string ListID { get; set; }
        /// <summary>
        /// The identification scheme version identifier of the Item classification identifier
        /// </summary>
        [XmlAttribute("listVersionID")]
        public string ListVersionID { get; set; }
    }
}