using BECOSOFT.Utilities.Collections;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace BECOSOFT.Web.Helpers {
    public class LogActionFilter : ActionFilterAttribute {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public override void OnActionExecuting(ActionExecutingContext filterContext) {
            Log(LogLevel.Info, "OnActionExecuting", filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext) {
            Log(LogLevel.Trace, "OnActionExecuted", filterContext);
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext) {
            Log(LogLevel.Trace, "OnResultExecuting", filterContext);
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext) {
            Log(LogLevel.Trace, "OnResultExecuted", filterContext);
        }

        private static void Log(LogLevel logLevel, string methodName, ControllerContext context) {
            if (!Logger.IsEnabled(logLevel)) { return; }
            var routeData = context.RouteData;
            var controllerName = routeData.Values["controller"];
            var actionName = routeData.Values["action"];
            var actionType = context.HttpContext.Request.HttpMethod;
            var valueBuilder = new StringBuilder();
            try {
                var url = context.HttpContext.Request.Url;
                var cleanedPath = GetCleanedPath(url);
                valueBuilder.Append("{0} controller: {1} action: {2} [{3}]\nOriginal Url: {4}",
                                        methodName, controllerName, actionName, actionType,
                                        cleanedPath);
                try {
                    valueBuilder.Append($" (Size: {context.HttpContext.Request.InputStream.Length})");
                } catch (Exception) { //
                }
                valueBuilder.AppendLine();
            } catch (Exception e) {
                Logger.Error(e);
            }
            var ctrlName = controllerName as string;
            var logRequestParameters = ConfigurationManager.AppSettings["LogRequestParameters"].To<bool>();
            if (!logRequestParameters) {
                Logger.Log(logLevel, valueBuilder.ToString());
                return;
            }

            StringBuilder parameterBuilder = null;
            if (context is ActionExecutingContext actionExecutingContext && actionExecutingContext.ActionParameters.HasAny()) {
                parameterBuilder = GetActionParameters(actionExecutingContext);
            }
            var hasParameterBuilder = parameterBuilder != null && parameterBuilder.HasValue();
            if (!hasParameterBuilder) {
                Logger.Log(logLevel, valueBuilder.ToString());
                return;
            }
            if (string.Equals(ctrlName, "ScanSessionApi", StringComparison.InvariantCultureIgnoreCase)) {
                Logger.Log(logLevel, valueBuilder.ToString());
                if (logLevel != LogLevel.Trace && Logger.IsDebugEnabled) {
                    Logger.Log(LogLevel.Debug, parameterBuilder.ToString);
                }
                return;
            }
            valueBuilder.Append(parameterBuilder);
            Logger.Log(logLevel, valueBuilder.ToString());

            return;

            KeyValueList<string, string> CleanUrlParts(Dictionary<string, string> queryParts, Uri url) {
                var cleanedParts = new KeyValueList<string, string>();
                foreach (var queryPart in queryParts) {
                    var urlDecodedValue = System.Net.WebUtility.UrlDecode(queryPart.Value);
                    string value;
                    if (urlDecodedValue != queryPart.Value) {
                        var tempUri = new Uri(url.GetLeftPart(UriPartial.Path));
                        var tempToCleanUri = tempUri.Append(urlDecodedValue);
                        var tempParts = tempToCleanUri.DecodeQueryParameters();
                        var tempCleanedParts = CleanUrlParts(tempParts, url);
                        var tempPath = tempUri.AppendQueryPart(tempCleanedParts);
                        value = System.Net.WebUtility.UrlEncode(tempPath.PathAndQuery);
                    } else {
                        value = queryPart.Key.Contains("Password", StringComparison.InvariantCultureIgnoreCase) ? "*****" : queryPart.Value;
                    }
                    cleanedParts.Add(queryPart.Key, value);
                }
                return cleanedParts;
            }
            string GetCleanedPath(Uri uri) {
                if (uri == null) { return "?"; }
                var queryParts = uri.DecodeQueryParameters();
                if (queryParts.IsEmpty()) { return uri.PathAndQuery; }
                var cleanedParts = CleanUrlParts(queryParts, uri);
                var newUri = new Uri(uri.GetLeftPart(UriPartial.Path));
                return newUri.AppendQueryPart(cleanedParts).PathAndQuery;
            }
        }

        private static StringBuilder GetActionParameters(ActionExecutingContext actionExecutingContext) {
            var parameterBuilder = new StringBuilder("with parameters:");
            foreach (var actionParameter in actionExecutingContext.ActionParameters) {
                var key = actionParameter.Key ?? "null";

                if (actionParameter.Value == null) {
                    parameterBuilder.Append($"\n{key}: null");
                    continue;
                }

                var type = actionParameter.Value.GetType();
                if (type.BaseType == typeof(HttpPostedFileBase)) { continue; }

                if (type == typeof(byte[])) {
                    parameterBuilder.AppendLine($"{key}: Size = {((byte[])actionParameter.Value).Length}.");
                    continue;
                }
                var value = GetStringRepresentation(actionParameter);

                if (!actionParameter.Key.IsNullOrWhiteSpace() && key.Contains("Password", StringComparison.InvariantCultureIgnoreCase)) { value = "*****"; }

                if (!actionParameter.Key.IsNullOrWhiteSpace() && key.Contains("returnUrl", StringComparison.InvariantCultureIgnoreCase)) {
                    var queryParameters = HttpUtility.ParseQueryString(value);
                    var passwordKeys = queryParameters.AllKeys.Where(k => k.Contains("Password", StringComparison.InvariantCultureIgnoreCase));
                    foreach (var passwordKey in passwordKeys) {
                        queryParameters[passwordKey] = "*****";
                    }

                    value = HttpUtility.UrlDecode(queryParameters.ToString());
                }
                parameterBuilder.AppendLine($"{key}: {value}");
            }
            return parameterBuilder;
        }

        private static string GetStringRepresentation(KeyValuePair<string, object> actionParameter) {
            string value;

            try {
                var serializer = new JsonSerializer { ContractResolver = LoggingContractResolver.Instance };
                var jToken = JToken.FromObject(actionParameter.Value, serializer);
                var json = new JObject { [actionParameter.Key] = jToken };
                value = CleanJson(json);
            } catch (Exception ex) {
                value = actionParameter.Value.ToString();
            }

            return value;
        }

        private static string CleanJson(JObject jsonObject) {
            foreach (var node in jsonObject) {
                if (node.Value.Type == JTokenType.Object) {
                    CleanJson((JObject) node.Value);
                } else if (node.Value.Type == JTokenType.Array) {
                    foreach (var i in node.Value) {
                        if (i.Type != JTokenType.Object) {
                            CleanValue(i, node.Key);
                            continue;
                        }
                        CleanJson((JObject)i);
                    }
                } else {
                    CleanToken(node);
                }
            }

            return jsonObject.ToString(Formatting.Indented);
        }

        private static void CleanToken(KeyValuePair<string, JToken> node) {
            CleanValue(node.Value, node.Key);
        }

        private static void CleanValue(JToken nodeValue, string nodeKey) {
            if (nodeKey.Contains("Password", StringComparison.InvariantCultureIgnoreCase)) {
                nodeValue.Replace("******");
            }
            if (nodeValue.Type == JTokenType.String) {
                var valueStr = nodeValue.ToObject<string>();
                if (valueStr != null && valueStr.StartsWith("data:image", StringComparison.InvariantCultureIgnoreCase)) {
                    nodeValue.Replace($"<img base64 of length {valueStr.Length}>");
                }
            }

            if (nodeKey.Equals("ReturnUrl")) {
                var tempUri = "https://www.temp.com" + nodeValue;
                var uri = new Uri(tempUri);
                var keyCollection = HttpUtility.ParseQueryString(uri.Query);
                var paramString = $"/{uri.LocalPath}?";
                var firstPart = true;
                foreach (var key in keyCollection.AllKeys) {
                    if (!firstPart) {
                        paramString += "&";
                    }

                    firstPart = false;
                    if (key.Contains("Password", StringComparison.InvariantCultureIgnoreCase)) {
                        paramString += $"{key}=****";
                    } else {
                        var value = keyCollection[key];
                        paramString += $"{key}={value}";
                    }
                }

                nodeValue.Replace(paramString);
            }
        }
    }
}