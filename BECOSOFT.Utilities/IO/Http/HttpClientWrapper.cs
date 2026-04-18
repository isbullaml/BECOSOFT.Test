using BECOSOFT.Utilities.Extensions;
using NLog;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BECOSOFT.Utilities.IO.Http {
    public class HttpClientWrapper : IHttpClientWrapper {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public HttpClient HttpClient { get; } = new HttpClient();

        public async Task<HttpResponseWrapper<TOut, TError>> Post<TIn, TOut, TError>(HttpRequestWrapper<TIn, TOut, TError> request)
            where TIn : class
            where TOut : class
            where TError : class {
            try {
                var (content, requestLogString) = request.ContentConverter(request.RequestObject);
                using (var response = await HttpClient.PostAsync(request.Url, content)) {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Logger.Log(request.LogLevel, "POST sent to {0} with content:\n{1}.\nResponse code: {2} with content\n{3}", request.Url, requestLogString, response.StatusCode, responseString);
                    return CreateResult(request, response, responseString);
                }
            } catch (Exception ex) {
                Logger.Log(request.ErrorLogLevel, ex);
                return new HttpResponseWrapper<TOut, TError> {
                    Error = ex.Message,
                };
            }
        }

        public async Task<HttpResponseWrapper<TOut, TError>> Get<TIn, TOut, TError>(HttpRequestWrapper<TIn, TOut, TError> request)
            where TIn : class
            where TOut : class
            where TError : class {
            try {
                using (var response = await HttpClient.GetAsync(request.Url)) {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Logger.Log(request.LogLevel, "GET sent to {0} with content:\n{1}.\nResponse code: {2}", request.Url, response.StatusCode, responseString);
                    return CreateResult(request, response, responseString);
                }
            } catch (Exception ex) {
                Logger.Log(request.ErrorLogLevel, ex);
                return new HttpResponseWrapper<TOut, TError> {
                    Error = ex.Message,
                };
            }
        }

        private static HttpResponseWrapper<TOut, TError> CreateResult<TIn, TOut, TError>(HttpRequestWrapper<TIn, TOut, TError> request, HttpResponseMessage response, string responseString)
            where TIn : class
            where TOut : class
            where TError : class {
            if (!response.IsSuccessStatusCode) {
                return new HttpResponseWrapper<TOut, TError> {
                    StatusCode = response.StatusCode,
                    Content = responseString,
                    Error = "Invalid response received",
                };
            }

            if (responseString.IsNullOrWhiteSpace()) {
                return new HttpResponseWrapper<TOut, TError> {
                    StatusCode = response.StatusCode,
                    Content = responseString,
                    IsSuccess = response.IsSuccessStatusCode,
                    Error = "Empty response received",
                };
            }

            try {
                var parsedResponse = request.ResponseConverter(responseString);
                return new HttpResponseWrapper<TOut, TError> {
                    StatusCode = response.StatusCode,
                    Content = responseString,
                    IsSuccess = response.IsSuccessStatusCode,
                    Response = parsedResponse,
                };
            } catch (Exception ex) {
                try {
                    var parsedResponse = request.ErrorResponseConverter(responseString);
                    return new HttpResponseWrapper<TOut, TError> {
                        StatusCode = response.StatusCode,
                        Content = responseString,
                        IsSuccess = response.IsSuccessStatusCode,
                        ErrorResponse = parsedResponse,
                    };
                } catch (Exception errorEx) {
                    Logger.Log(request.ErrorLogLevel, ex);
                    return new HttpResponseWrapper<TOut, TError> {
                        StatusCode = response.StatusCode,
                        Error = errorEx.Message,
                        Content = responseString,
                    };
                }
            }
        }

        public void Dispose() {
            HttpClient?.Dispose();
        }
    }
}