using System.Globalization;
using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLQuantity {
        /// <summary>
        /// The percentage that may be used, in conjunction with the document level allowance base amount, to calculate the document level allowance or charge amount. To state 20%, use value 20. 
        /// </summary>
        [XmlText]
        public string QuantityString {
            get => Quantity.ToString(CultureInfo.InvariantCulture);
            set {
                decimal temp;
                if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out temp)) {
                    Quantity = temp;
                }
            }
        }

        /// <summary>
        /// The percentage that may be used, in conjunction with the document level allowance base amount, to calculate the document level allowance or charge amount. To state 20%, use value 20. 
        /// </summary>
        [XmlIgnore]
        public decimal Quantity { get; set; }

        /// <summary>
        /// The unit of measure that applies to the invoiced quantity.
        /// Codes for unit of packaging from UNECE Recommendation No. 21 can be used in accordance
        /// with the descriptions in the "Intro" section of UN/ECE Recommendation 20, Revision 11 (2015):
        /// The 2 character alphanumeric code values in UNECE Recommendation 21 shall be used.
        /// To avoid duplication with existing code values in UNECE Recommendation No. 20, each code
        /// value from UNECE Recommendation 21 shall be prefixed with an “X”, resulting in a 3 alphanumeric code when used as a unit of measure.
        ///
        /// List: https://docs.peppol.eu/poacc/billing/3.0/codelist/UNECERec21/
        /// </summary>
        [XmlAttribute("unitCode")]
        public string UnitCode { get; set; }

        public UBLQuantity() {
        }

        public UBLQuantity(decimal quantity, string unitCode) {
            Quantity = quantity;
            UnitCode = unitCode;
        }
    }
}