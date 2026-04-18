using BECOSOFT.Utilities.Extensions;
using System;

namespace BECOSOFT.ThirdParty.EuropeanCommission.Models {
    public class EoriValidationResponse {
        public string EORI { get; set; }
        public int Status { get; set; }
        public string StatusDescription { get; set; }
        public string ErrorReason { get; set; }
        public string Name { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public EoriAddress Address { get; }
        public Exception Exception { get; }
        public string Error { get; }
        public string ErrorMessage { get; }
        public DateTime RequestDate { get; }
        public bool HasError => (Status != 0 && !Error.IsNullOrWhiteSpace()) || Exception != null;
        public bool IsValid => Status == 0 && ErrorMessage != "Valid";

        internal EoriValidationResponse(Exception exception) {
            Exception = exception;
            ErrorReason = exception.Message;
        }
        
        internal EoriValidationResponse(string error, string errorMessage) {
            Error = error;
            ErrorMessage = errorMessage;
        }

        internal EoriValidationResponse(string eori, int status, string statusDescr, string name, string street, string postalCode, string city, string country, DateTime requestDate) {
            EORI = eori;
            Status = status;
            StatusDescription = statusDescr;
            Name = name;
            Street = street;
            PostalCode = postalCode;
            City = city;
            Country = country;
            RequestDate = requestDate;
            Address = EoriAddressParser.Parse(country, street, postalCode, city);
        }
    }
}