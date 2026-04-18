using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Models.Security;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BECOSOFT.Utilities.Security.OAuth {
    public abstract class OAuthClient<T, TOAuthState> : IOAuthClient<T, TOAuthState>
        where T : OAuthClientParameters 
        where TOAuthState : OAuthStateResult {

        private readonly ILogger _logger;

        /// <summary>
        /// Setting <see cref="HttpMessageHandler"/> makes mocking a <see cref="HttpClient"/> possible.
        /// </summary>
        public HttpMessageHandler HttpMessageHandler { get; set; }

        protected OAuthClient(ILogger logger) {
            _logger = logger;
        }

        public async Task<TOAuthState> Authorize(T parameters) {
            TOAuthState state;

            try {
                state = await RefreshAuthorization(parameters);
            } catch (Exception e) {
                _logger.Warn(e);
                state = null;
            }

            if (state?.AccessToken == null) {
                var authorizationUri = await HandlePreAuthorization(parameters);
                state = await HandleTokenRefresh(parameters, state, authorizationUri);
            }

            return state;
        }

        protected virtual Task<Uri> HandlePreAuthorization(T parameters) {
            return Task.FromResult(default(Uri));
        }

        protected async Task<TOAuthState> RefreshAuthorization(T parameters) {
            if (!CanRefresh(parameters)) { return null; }
            var refreshParameters = GetRefreshParameters(parameters);
            return await MakeTokenRequest(parameters, refreshParameters, true);
        }

        protected virtual bool CanRefresh(T parameters) => true;
        protected virtual bool CanMakeTokenRequest(T parameters, Uri authorizationUri) => true;

        protected async Task<TOAuthState> HandleTokenRefresh(T parameters, TOAuthState state, Uri authorizationUri) {
            if (!CanMakeTokenRequest(parameters, authorizationUri)) { return state; }
            var tokenParameters = GetTokenParameters(parameters, authorizationUri);
            return await MakeTokenRequest(parameters, tokenParameters, false);
        }

        protected abstract Dictionary<string, string> GetRefreshParameters(T parameters);

        protected abstract Dictionary<string, string> GetTokenParameters(T parameters, Uri authorizationUri);

        protected virtual string GetJsonTokenParameters(T parameters, Dictionary<string, string> extraParameters, bool isRefresh) => null;

        protected virtual void AddExtraUrlParameters(Dictionary<string, string> urlParameters, T parameters) {
        }

        protected virtual Uri GetEndpoint(OAuthServerInfo serverInfo, bool isRefresh) {
            return serverInfo.TokenEndpoint;
        }

        private async Task<TOAuthState> MakeTokenRequest(T parameters, Dictionary<string, string> extraParameters, bool isRefresh) {
            var urlParameters = new Dictionary<string, string>();
            AddExtraUrlParameters(urlParameters, parameters);
            urlParameters.AddRange(extraParameters);
            var endpoint = GetEndpoint(parameters.ServerInfo, isRefresh);
            using (var client = CreateClient()) {
                SetClientHeaders(client.DefaultRequestHeaders, parameters);
                string content = null;
                HttpResponseMessage response = null;
                try {
                    if (parameters.UseGetForToken) {
                        response = await client.GetAsync(endpoint);
                    } else if (parameters.UsePostWithJsonBodyForToken) {
                        var json = GetJsonTokenParameters(parameters, extraParameters, isRefresh);
                        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                        response = await client.PostAsync(endpoint, stringContent);
                    } else {
                        var formUrlEncodedContent = new FormUrlEncodedContent(urlParameters);
                        response = await client.PostAsync(endpoint, formUrlEncodedContent);
                    }
                    content = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    var state = JsonConvert.DeserializeObject<TOAuthState>(content);
                    if (state != null) {
                        if (response.Headers.Date.HasValue) {
                            state.UtcResponseDate = response.Headers.Date.Value.UtcDateTime;
                        }
                        if (state.UtcResponseDate.HasValue && state.ExpiresInSeconds.HasValue) {
                            state.UtcExpirationDate = state.UtcResponseDate.Value.AddSeconds(state.ExpiresInSeconds.Value);
                        } else if (state.ExpiresInSeconds.HasValue) {
                            if (response.Headers.Date.HasValue) {
                                state.UtcExpirationDate = response.Headers.Date.Value.UtcDateTime.AddSeconds(state.ExpiresInSeconds.Value);
                            } else {
                                state.UtcExpirationDate = DateTime.UtcNow.AddSeconds(state.ExpiresInSeconds.Value);
                            }
                        } else {
                            state.UtcResponseDate = ParseUtcResponseDate(state, response.Headers);
                        }
                    }
                    return state;
                } catch (Exception e) {
                    _logger.Error("Failed token request to '{0}'.", endpoint);
                    if (response != null && response.Headers.TryGetValues("Reason", out var values)) {
                        _logger.Error("Reason:");
                        foreach (var value in values) {
                            _logger.Error(value);
                        }
                    }
                    _logger.Error(e);
                    if (content.HasValue()) {
                        _logger.Error(content);
                    }
                    return null;
                }
            }
        }

        public virtual DateTime? ParseUtcResponseDate(TOAuthState state, HttpResponseHeaders responseHeaders) => state.UtcResponseDate;

        protected virtual void SetClientHeaders(HttpRequestHeaders clientHeaders, T parameters) {
        }

        private HttpClient CreateClient() => HttpMessageHandler == null ? new HttpClient() : new HttpClient(HttpMessageHandler);
    }
}