using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Helpers;
using System;
using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public abstract class UBLDocument {
        
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string UBLVersionID { get; set; }
        public bool ShouldSerializeUBLVersionID() => UBLVersionID.HasValue();
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string CustomizationID { get; set; }

        public bool ShouldSerializeCustomizationID() => CustomizationID.HasValue();
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string ProfileID { get; set; }

        public bool ShouldSerializeProfileID() => ProfileID.HasValue();

        /// <summary>
        /// Document identifier
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public UBLIdentifier ID { get; set; }

        [XmlElement("IssueDate", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string IssueDateString {
            get => IssueDateTime.ToString("yyyy-MM-dd");
            set => IssueDateTime = DateTimeHelpers.Parse(value).Add(IssueDateTime.TimeOfDay);
        }

        [XmlIgnore]
        public DateTime IssueDateTime { get; set; }

        [XmlElement("CopyIndicator", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public bool? IsCopy { get; set; }

        public bool ShouldSerializeIsCopy() => IsCopy.HasValue;

        [XmlElement("UUID", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public Guid? UniqueIdentifier { get; set; }

        public bool ShouldSerializeUniqueIdentifier() => UniqueIdentifier.HasValue;
    }
}