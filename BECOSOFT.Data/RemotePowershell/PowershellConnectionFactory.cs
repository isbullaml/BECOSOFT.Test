using BECOSOFT.Data.Models.Powershell;
using BECOSOFT.Utilities.Extensions;
using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;

namespace BECOSOFT.Data.RemotePowershell {
    /// <summary>
    /// Connectionfactory for remote Powershell
    /// </summary>
    public class PowershellConnectionFactory : IPowershellConnectionFactory {
        /// <summary>
        /// Creates a remote Powershell connection from a Powershell-Info object
        /// </summary>
        /// <param name="powershellInfo">The Powershell-Info</param>
        /// <returns>The connection</returns>
        public WSManConnectionInfo CreateConnection(PowershellInfo powershellInfo) {
            var uri = GetUri(powershellInfo.Address, powershellInfo.Port);
            WSManConnectionInfo info;
            if (!powershellInfo.Username.IsNullOrWhiteSpace()) {
                var cred = ToCredential(powershellInfo.Username, powershellInfo.Password);
                info = new WSManConnectionInfo(uri, powershellInfo.PsShellUri, cred);
            } else {
                info = new WSManConnectionInfo(uri);
            }
            info.AuthenticationMechanism = powershellInfo.AuthenticationMechanism;
            info.SkipCACheck = powershellInfo.SkipCACheck;
            info.SkipCNCheck = powershellInfo.SkipCNCheck;
            info.NoEncryption = powershellInfo.NoEncryption;
            return info;
        }

        /// <summary>
        /// Converts a username and password to a PSCredential
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <returns>The PSCredential</returns>
        private static PSCredential ToCredential(string username, string password) {
            return new PSCredential(username, ToSecureString(password));
        }

        /// <summary>
        /// Converts a string to a secure string
        /// </summary>
        /// <param name="text">The original string</param>
        /// <returns>The secure string</returns>
        private static SecureString ToSecureString(string text) {
            var secure = new SecureString();
            foreach (var character in text) {
                secure.AppendChar(character);
            }
            return secure;
        }

        /// <summary>
        /// Converts a URL and port to an URI
        /// </summary>
        /// <param name="url">The URL</param>
        /// <param name="port">The port</param>
        /// <returns>The URI</returns>
        private static Uri GetUri(string url, int port) {
            var uriBuilder = new UriBuilder(url) {
                Port = port
            };
            return uriBuilder.Uri;
        }
    }
}