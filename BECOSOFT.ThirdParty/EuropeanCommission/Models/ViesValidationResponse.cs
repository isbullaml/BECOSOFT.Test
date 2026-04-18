using BECOSOFT.Utilities.Extensions;
using System;

namespace BECOSOFT.ThirdParty.EuropeanCommission.Models {
    public class ViesValidationResponse {
        public string CountryCode { get; }
        public string VatNumber { get; }
        public DateTime RequestDate { get; }
        public bool Valid { get; }
        public string Name { get; }
        public string Error { get; }
        public string ErrorMessage { get; }
        public ViesAddress Address { get; }
        public Exception Exception { get; }

        public bool HasError => (!Valid && !Error.IsNullOrWhiteSpace()) || Exception != null;

        internal ViesValidationResponse(string error, string errorMessage) {
            Error = error;
            ErrorMessage = errorMessage;
        }

        internal ViesValidationResponse(Exception exception) {
            Exception = exception;
            Error = exception.Message;
            ErrorMessage = "";
        }

        internal ViesValidationResponse(string countryCode, string vatNumber, DateTime requestDate,
                                        bool valid, string name, string address) {
            CountryCode = countryCode;
            VatNumber = vatNumber;
            RequestDate = requestDate;
            Valid = valid;
            Name = name;
            Address = ViesAddressParser.Parse(CountryCode, address);
        }
    }

    public class ViesAddress {
        public string CountryCode { get; }
        public string Address { get; }
        public string Street { get; set; } = "";
        public string PostalCode { get; set; } = "";
        public string Place { get; set; } = "";
        public string Number { get; set; } = "";
        public string Box { get; set; } = "";
        public string Province { get; set; } = "";
        public string AddressLine { get; set; } = "";

        public ViesAddress(string countryCode, string address) {
            CountryCode = countryCode;
            Address = address;
        }
    }
}