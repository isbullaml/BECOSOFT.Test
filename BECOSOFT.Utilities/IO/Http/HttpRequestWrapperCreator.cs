using BECOSOFT.Utilities.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;

namespace BECOSOFT.Utilities.IO.Http {
    public static class HttpRequestWrapperCreator {
        public static class Json {
            public static HttpRequestWrapper<string, TOut, TError> Get<TOut, TError>(Uri url)
                where TOut : class
                where TError : class =>
                new HttpRequestWrapper<string, TOut, TError> {
                    Url = url,
                    ResponseConverter = JsonConvert.DeserializeObject<TOut>,
                    ErrorResponseConverter = JsonConvert.DeserializeObject<TError>,
                    ContentConverter = r => {
                        var json = JsonConvert.SerializeObject(r, new JsonSerializerSettings {
                            DefaultValueHandling = DefaultValueHandling.Ignore,
                        });

                        return (new StringContent(json, Encoding.UTF8, "application/json"), json);
                    },
                };

            public static HttpRequestWrapper<string, TOut, string> Get<TOut>(Uri url)
                where TOut : class =>
                Get<TOut, string>(url);

            public static HttpRequestWrapper<string, string, string> Get(Uri url) => Get<string>(url);

            public static HttpRequestWrapper<TIn, TOut, TError> Post<TIn, TOut, TError>(Uri url, TIn request)
                where TIn : class
                where TOut : class
                where TError : class =>
                new HttpRequestWrapper<TIn, TOut, TError> {
                    Url = url,
                    RequestObject = request,
                    ContentConverter = r => {
                        var json = JsonConvert.SerializeObject(r, new JsonSerializerSettings {
                            DefaultValueHandling = DefaultValueHandling.Ignore,
                        });

                        return (new StringContent(json, Encoding.UTF8, "application/json"), json);
                    },
                    ResponseConverter = JsonConvert.DeserializeObject<TOut>,
                    ErrorResponseConverter = JsonConvert.DeserializeObject<TError>,
                };

            public static HttpRequestWrapper<TIn, TOut, string> Post<TIn, TOut>(Uri url, TIn request)
                where TIn : class
                where TOut : class =>
                Post<TIn, TOut, string>(url, request);

            public static HttpRequestWrapper<TIn, string, string> Post<TIn>(Uri url, TIn request)
                where TIn : class =>
                Post<TIn, string>(url, request);

            public static HttpRequestWrapper<string, string, string> Post(Uri url) => Post(url, "");
        }

        public static class Xml {
            public static HttpRequestWrapper<string, TOut, TError> Get<TOut, TError>(Uri url)
                where TOut : class
                where TError : class =>
                new HttpRequestWrapper<string, TOut, TError> {
                    Url = url,
                    ContentConverter = r => {
                        var serializer = new XmlSerializer<object>();
                        var xml = serializer.SerializeToString(r);
                        return (new StringContent(xml, Encoding.UTF8, "application/xml"), xml);
                    },
                    ResponseConverter = r => {
                        var serializer = new XmlSerializer<TOut>();
                        return serializer.DeserializeToModel(r);
                    },
                    ErrorResponseConverter = r => {
                        var serializer = new XmlSerializer<TError>();
                        return serializer.DeserializeToModel(r);
                    },
                };

            public static HttpRequestWrapper<string, TOut, string> Get<TOut>(Uri url)
                where TOut : class =>
                Get<TOut, string>(url);

            public static HttpRequestWrapper<string, string, string> Get(Uri url) => Get<string>(url);

            public static HttpRequestWrapper<TIn, TOut, TError> Post<TIn, TOut, TError>(Uri url, TIn request)
                where TIn : class
                where TOut : class
                where TError : class =>
                new HttpRequestWrapper<TIn, TOut, TError> {
                    Url = url,
                    RequestObject = request,
                    ContentConverter = r => {
                        var serializer = new XmlSerializer<TIn>();
                        var xml = serializer.SerializeToString(r);
                        return (new StringContent(xml, Encoding.UTF8, "application/xml"), xml);
                    },
                    ResponseConverter = r => {
                        var serializer = new XmlSerializer<TOut>();
                        return serializer.DeserializeToModel(r);
                    },
                    ErrorResponseConverter = r => {
                        var serializer = new XmlSerializer<TError>();
                        return serializer.DeserializeToModel(r);
                    },
                };

            public static HttpRequestWrapper<TIn, TOut, string> Post<TIn, TOut>(Uri url, TIn request)
                where TIn : class
                where TOut : class =>
                Post<TIn, TOut, string>(url, request);

            public static HttpRequestWrapper<TIn, string, string> Post<TIn>(Uri url, TIn request)
                where TIn : class =>
                Post<TIn, string>(url, request);

            public static HttpRequestWrapper<string, string, string> Post(Uri url) => Post(url, "");
        }
    }
}