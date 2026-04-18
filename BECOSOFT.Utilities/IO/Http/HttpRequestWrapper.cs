using NLog;
using System;
using System.Net.Http;

namespace BECOSOFT.Utilities.IO.Http {
    public class HttpRequestWrapper<TIn, TOut, TError> where TIn : class
                                                       where TOut : class
                                                       where TError : class {
        public Uri Url { get; set; }

        public TIn RequestObject { get; set; }

        public Func<TIn, (HttpContent Content, string RequestLogString)> ContentConverter { get; set; }

        public Func<string, TOut> ResponseConverter { get; set; }

        public Func<string, TError> ErrorResponseConverter { get; set; }

        public Func<TOut, bool> IsValidResponse { get; set; }

        public LogLevel LogLevel { get; set; } = LogLevel.Info;

        public LogLevel ErrorLogLevel { get; set; } = LogLevel.Error;
    }
}