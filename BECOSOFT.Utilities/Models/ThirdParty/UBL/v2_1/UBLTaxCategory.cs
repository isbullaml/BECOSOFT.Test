using BECOSOFT.Utilities.Extensions;
using System.Globalization;
using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLTaxCategory {
        /// <summary>
        /// A coded identification of what VAT category applies to the document level allowance or charge.
        ///
        /// https://docs.peppol.eu/poacc/billing/3.0/codelist/UNCL5305/
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLIdentifier ID { get; set; }

        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string Name { get; set; }

        public bool ShouldSerializeName() => Name.HasValue();

        [XmlElement("Percent", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string PercentString {
            get => Percent?.ToString("F0", CultureInfo.InvariantCulture);
            set {
                decimal temp;
                decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out temp);
                Percent = temp;
            }
        }

        /// <summary>
        /// The VAT rate, represented as percentage that applies to the document level allowance or charge.
        /// </summary>
        [XmlIgnore]
        public decimal? Percent { get; set; }

        /// <summary>
        /// https://docs.peppol.eu/poacc/billing/3.0/codelist/vatex/
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string TaxExemptionReasonCode { get; set; }

        public bool ShouldSerializeTaxExemptionReasonCode() => TaxExemptionReasonCode.HasValue();

        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string TaxExemptionReason { get; set; }

        public bool ShouldSerializeTaxExemptionReason() => TaxExemptionReason.HasValue();

        /// <summary>
        /// Mandatory element. Use “VAT”
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public UBLTaxScheme TaxScheme { get; set; } = new UBLTaxScheme(new UBLIdentifier("VAT"));
    }
}