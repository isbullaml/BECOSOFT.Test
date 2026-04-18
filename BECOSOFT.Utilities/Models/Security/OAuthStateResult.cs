using System;

namespace BECOSOFT.Utilities.Models.Security {
    public abstract class OAuthStateResult {
        public abstract string RefreshToken { get; set; }
        public abstract string AccessToken { get; set; }
        public abstract string TokenType { get; set; }
        public abstract string Scope { get; set; }
        public abstract int? ExpiresInSeconds { get; set; }

        /// <summary>
        /// UTC <see cref="DateTime"/> of the http response
        /// </summary>
        public DateTime? UtcResponseDate { get; set; }

        /// <summary>
        /// UTC <see cref="DateTime"/> that denotes the expiration date of the token based on <see cref="UtcResponseDate"/> and <see cref="ExpiresInSeconds"/>, if both have a value.
        /// </summary>
        public DateTime? UtcExpirationDate { get; set; }
    }
}