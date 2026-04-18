using BECOSOFT.Utilities.Extensions.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLTaxTotal {
        /// <summary>
        /// The total VAT amount for the Invoice or the VAT total amount expressed in the accounting currency accepted or required in the country of the Seller.
        /// Must be rounded to maximum 2 decimals
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLAmount TaxAmount { get; set; }

        /// <summary>
        /// A group of business terms providing information about VAT breakdown by different categories, rates and exemption reasons 
        /// </summary>
        [XmlElement("TaxSubtotal", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public List<UBLTaxSubtotal> TaxSubtotals { get; set; }

        public bool ShouldSerializeTaxSubtotals() => TaxSubtotals.HasAny();
    }
}