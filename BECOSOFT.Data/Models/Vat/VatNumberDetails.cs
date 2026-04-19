using BECOSOFT.Data.Attributes;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using System;
using System.Diagnostics;

namespace BECOSOFT.Data.Models.Thirdparty {
    /// <summary>
    /// Represents VAT number validation details from external services
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [Table(Schema.Thirdparty, "VatNumberDetails", "Id")]
    public class VatNumberDetails : BaseEntity, ICacheableResult {
        /// <summary>
        /// The VAT number (including country code)
        /// </summary>
        [Column]
        public string VatNumber { get; set; }

        /// <summary>
        /// The country code (e.g., BE, NL, DE)
        /// </summary>
        [Column]
        public string CountryCode { get; set; }

        /// <summary>
        /// The company/person name
        /// </summary>
        [Column]
        public string Name { get; set; }

        /// <summary>
        /// The full address
        /// </summary>
        [Column]
        public string Address { get; set; }

        /// <summary>
        /// The street name
        /// </summary>
        [Column]
        public string Street { get; set; }

        /// <summary>
        /// The postal code
        /// </summary>
        [Column]
        public string PostalCode { get; set; }

        /// <summary>
        /// The place/city name
        /// </summary>
        [Column]
        public string Place { get; set; }

        /// <summary>
        /// The house/building number
        /// </summary>
        [Column]
        public string Number { get; set; }

        /// <summary>
        /// The box/apartment number
        /// </summary>
        [Column]
        public string Box { get; set; }

        /// <summary>
        /// The province/state
        /// </summary>
        [Column]
        public string Province { get; set; }

        /// <summary>
        /// Additional address line information
        /// </summary>
        [Column]
        public string AddressLine { get; set; }

        /// <summary>
        /// Indicates if the VAT number is valid
        /// </summary>
        [Column]
        public bool IsValid { get; set; }

        /// <summary>
        /// The date and time when this record was last updated
        /// </summary>
        [Column]
        public DateTime LastUpdated { get; set; }

        private string DebuggerDisplay => $"{VatNumber} - {Name} ({CountryCode})";
    }
}