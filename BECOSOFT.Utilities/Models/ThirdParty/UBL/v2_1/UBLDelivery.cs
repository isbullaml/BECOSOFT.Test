using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLDelivery {
        [XmlElement("ActualDeliveryDate", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string ActualDeliveryDateString {
            get => ActualDeliveryDate?.ToString("yyyy-MM-dd");
            set => ActualDeliveryDate = DateTimeHelpers.Parse(value);
        }

        public bool ShouldSerializeActualDeliveryDateString() => ActualDeliveryDate.HasValue;

        /// <summary>
        /// The date when the payment is due.Format "YYYY-MM-DD". In case the Amount due for payment (BT-115) is positive,
        /// either the Payment due date (BT-9) or the Payment terms (BT-20) shall be present.
        /// </summary>
        [XmlIgnore]
        public DateTime? ActualDeliveryDate { get; set; }

        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public UBLDeliveryLocation DeliveryLocation { get; set; }

        public bool ShouldSerializeDeliveryLocation() => DeliveryLocation != null;

        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public UBLParty PayeeParty { get; set; }

        public bool ShouldSerializePayeeParty() => PayeeParty != null;

        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public List<UBLDeliveryTerm> DeliveryTerms { get; set; }

        public bool ShouldSerializeDeliveryTerms() => DeliveryTerms.HasAny();
    }
}