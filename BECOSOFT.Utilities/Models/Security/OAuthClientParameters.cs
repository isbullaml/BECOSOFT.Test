namespace BECOSOFT.Utilities.Models.Security {
    public abstract class OAuthClientParameters {
        protected OAuthClientParameters(OAuthServerInfo serverInfo) {
            ServerInfo = serverInfo;
        }

        public OAuthServerInfo ServerInfo { get; }

        public bool UseGetForToken { get; set; }
        public bool UsePostWithJsonBodyForToken { get; set; }
    }
}