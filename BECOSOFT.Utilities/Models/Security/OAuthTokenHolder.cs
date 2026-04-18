using System;

namespace BECOSOFT.Utilities.Models.Security {
    /// <summary>
    /// OAuth token holder for acquired <see cref="RefreshToken"/> and <see cref="AccessToken"/> strings.
    /// </summary>
    public class OAuthTokenHolder {
        /// <summary>
        /// OAuth refresh token
        /// </summary>
        public string RefreshToken { get; }
        /// <summary>
        /// OAuth access token
        /// </summary>
        public string AccessToken { get; }

        public DateTime? UtcExpirationDate { get; set; }

        public OAuthTokenHolder(string refreshToken, string accessToken) {
            RefreshToken = refreshToken;
            AccessToken = accessToken;
        }
    }
}