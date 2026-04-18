using System;

namespace BECOSOFT.Utilities.Models.Security {
    /// <summary>
    /// Contains the Token and Authorization end points
    /// </summary>
    public class OAuthServerInfo {
        /// <summary>
        /// Gets or sets the Authorization Server URL from which an Access Token is requested by the Client.
        /// </summary>
        /// <value>An HTTPS URL.</value>
        public Uri TokenEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the Authorization Server URL where the Client (re)directs the User
        /// to make an authorization request.
        /// </summary>
        /// <value>An HTTPS URL.</value>
        public Uri AuthorizationEndpoint { get; set; }
    }
}