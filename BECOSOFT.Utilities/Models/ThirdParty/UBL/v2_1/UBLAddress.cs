using BECOSOFT.Utilities.Extensions;
using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLAddress {
        /// <summary>
        /// The main address line in an address.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string StreetName { get; set; }

        public bool ShouldSerializeStreetName() => StreetName.HasValue();

        /// <summary>
        /// An additional address line in an address that can be used to give further details supplementing the main line.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string AdditionalStreetName { get; set; }

        public bool ShouldSerializeAdditionalStreetName() => StreetName.HasValue();

        /// <summary>
        /// The common name of the city, town or village, where the Seller address is located.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string CityName { get; set; }

        public bool ShouldSerializeCityName() => CityName.HasValue();

        /// <summary>
        /// The identifier for an addressable group of properties according to the relevant postal service. 
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string PostalZone { get; set; }

        public bool ShouldSerializePostalZone() => PostalZone.HasValue();

        /// <summary>
        /// The subdivision of a country.
        /// </summary>
        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
        public string CountrySubentity { get; set; }

        public bool ShouldSerializeCountrySubentity() => CountrySubentity.HasValue();

        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public UBLAddressLine AddressLine { get; set; }

        public bool ShouldSerializeAddressLine() => AddressLine != null;

        [XmlElement(Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public UBLCountry Country { get; set; }
    }
}