using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Helpers;
using System;
using System.Globalization;
using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLPaymentTerms {
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string Note { get; set; }

        public bool ShouldSerializeNote() => Note.HasValue();
        
        [XmlElement("SettlementDiscountPercent", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string SettlementDiscountPercentString {
            get => SettlementDiscountPercent?.ToString(CultureInfo.InvariantCulture);
            set {
                decimal temp;
                decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out temp);
                SettlementDiscountPercent = temp;
            }
        }

        [XmlIgnore]
        public decimal? SettlementDiscountPercent { get; set; }

        public bool ShouldSerializeSettlementDiscountPercentString() => SettlementDiscountPercent.HasValue;
        
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLAmount Amount { get; set; }
        
        [XmlElement("PaymentDueDate", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string PaymentDueDateString {
            get => PaymentDueDate?.ToString("yyyy-MM-dd");
            set => PaymentDueDate = DateTimeHelpers.Parse(value);
        }
        
        [XmlIgnore]
        public DateTime? PaymentDueDate { get; set; }

        public bool ShouldSerializePaymentDueDateString() => PaymentDueDate.HasValue;
    }
}