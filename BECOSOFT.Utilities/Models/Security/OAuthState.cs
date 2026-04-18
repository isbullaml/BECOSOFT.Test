using Newtonsoft.Json;
using System;

namespace BECOSOFT.Utilities.Models.Security {
    [Serializable]
    public class OAuthState : OAuthStateResult {
        [JsonProperty("refresh_token")]
        public override string RefreshToken { get; set; }

        [JsonProperty("access_token")]
        public override string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public override string TokenType { get; set; }

        [JsonProperty("scope")]
        public override string Scope { get; set; }

        [JsonProperty("expires_in")]
        public override int? ExpiresInSeconds { get; set; }
    }
}
