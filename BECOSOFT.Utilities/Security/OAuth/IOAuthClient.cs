using BECOSOFT.Utilities.Models.Security;
using System.Net.Http;
using System.Threading.Tasks;

namespace BECOSOFT.Utilities.Security.OAuth {
    public interface IOAuthClient<in T, TOAuthState> where T : OAuthClientParameters where TOAuthState : OAuthStateResult {
        HttpMessageHandler HttpMessageHandler { get; set; }
        Task<TOAuthState> Authorize(T parameters);
    }
}