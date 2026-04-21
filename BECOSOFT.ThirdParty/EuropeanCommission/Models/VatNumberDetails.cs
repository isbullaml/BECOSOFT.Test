using BECOSOFT.Data.Attributes;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using System;

namespace BECOSOFT.ThirdParty.EuropeanCommission.Models {
    [Table(Schema.Thirdparty, "VatNumberDetails", "Id")]
    public class VatNumberDetails : BaseEntity {
        [Column]
        public string VatNumber { get; set; }

        [Column]
        public string CountryCode { get; set; }

        [Column]
        public string Name { get; set; }

        [Column]
        public string Address { get; set; }

        [Column]
        public string Street { get; set; }

        [Column]
        public string PostalCode { get; set; }

        [Column]
        public string Place { get; set; }

        [Column]
        public string Number { get; set; }

        [Column]
        public string Box { get; set; }

        [Column]
        public string Province { get; set; }

        [Column]
        public string AddressLine { get; set; }

        [Column]
        public bool IsValid { get; set; }

        [Column]
        public DateTime LastUpdated { get; set; }
    }
}
