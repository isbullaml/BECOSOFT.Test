using System;
using System.Threading.Tasks;

namespace BECOSOFT.Utilities.IO.Http {
    public interface IHttpClientWrapper : IDisposable {
        Task<HttpResponseWrapper<TOut, TError>> Post<TIn, TOut, TError>(HttpRequestWrapper<TIn, TOut, TError> request) where TIn : class where TOut : class where TError : class;
        Task<HttpResponseWrapper<TOut, TError>> Get<TIn, TOut, TError>(HttpRequestWrapper<TIn, TOut, TError> request) where TIn : class where TOut : class where TError : class;
    }
}