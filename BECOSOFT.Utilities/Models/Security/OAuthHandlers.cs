using System;

namespace BECOSOFT.Utilities.Models.Security {
    public class OAuthHandlers {
        public delegate Uri Login(Uri redirectUri, Uri authorizationUri);
    }
}