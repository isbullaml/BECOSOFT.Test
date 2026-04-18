using System.Net;

namespace BECOSOFT.Utilities.IO.Http {
    public class HttpResponseWrapper<T, TError> : HttpResponseWrapper<T> {
        public TError ErrorResponse { get; set; }
    }

    public class HttpResponseWrapper<T> : HttpResponseWrapper {
        public T Response { get; set; }
    }

    public class HttpResponseWrapper {
        public bool IsSuccess { get; set; }

        public string Content { get; set; }

        public string Error { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}