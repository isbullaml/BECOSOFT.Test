namespace BECOSOFT.ThirdParty.EuropeanCommission.Models {
    public class EoriAddress {
        public string Country { get; }
        public string PostalCode { get; }
        public string Place { get; }
        public string Street { get; set; } = "";
        public string Number { get; set; } = "";
        public string Box { get; set; } = "";
        public string Province { get; set; } = "";
        public string AddressLine { get; set; } = "";

        public EoriAddress(string country, string postalCode, string city) {
            Country = country;
            PostalCode = postalCode;
            Place = city;
        }
    }
}