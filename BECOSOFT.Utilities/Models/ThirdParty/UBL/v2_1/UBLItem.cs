using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    /// <summary>
    /// A group of business terms providing information about the goods and services invoiced. 
    /// </summary>
    public class UBLItem {
        /// <summary>
        /// A description for an item.The item description allows for describing the item and its features in more detail than the Item name.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string Description { get; set; }

        public bool ShouldSerializeDescription() => Description.HasValue();

        /// <summary>
        /// A name for an item.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string Name { get; set; }
        
        /// <summary>
        /// An identifier, assigned by the Seller, for the item.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public UBLIdentification SellersItemIdentification { get; set; }

        public bool ShouldSerializeSellersItemIdentification() => SellersItemIdentification != null;
        /// <summary>
        /// An item identifier based on a registered scheme.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public UBLIdentification StandardItemIdentification { get; set; }

        public bool ShouldSerializeStandardItemIdentification() => StandardItemIdentification != null;
        
        /// <summary>
        /// The code identifying the country from which the item originates.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public UBLCountry OriginCountry { get; set; }

        public bool ShouldSerializeOriginCountry() => OriginCountry != null;
        
        [XmlElement("CommodityClassification", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public List<UBLCommodityClassification> CommodityClassifications { get; set; }

        public bool ShouldSerializeCommodityClassifications() => CommodityClassifications.HasAny();
        
        /// <summary>
        /// A group of business terms providing information about the VAT applicable for the goods and services invoiced on the Invoice line. 
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public UBLTaxCategory ClassifiedTaxCategory { get; set; }
        
        /// <summary>
        /// A group of business terms providing information about properties of the goods and services invoiced. 
        /// </summary>
        [XmlElement("AdditionalItemProperty", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public List<UBLAdditionalItemProperty> AdditionalItemProperties { get; set; }

        public bool ShouldSerializeAdditionalItemProperties() => AdditionalItemProperties.HasAny();
    }
}