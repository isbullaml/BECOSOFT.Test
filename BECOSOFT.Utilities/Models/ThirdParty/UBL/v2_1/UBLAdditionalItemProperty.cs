using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    /// <summary>
    /// A group of business terms providing information about properties of the goods and services invoiced. 
    /// </summary>
    public class UBLAdditionalItemProperty {
        /// <summary>
        /// The name of the attribute or property of the item.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string Name { get; set; }
        /// <summary>
        /// The value of the attribute or property of the item.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string Value { get; set; }
    }
}