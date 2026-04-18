using Autofac;
using Autofac.Integration.Mvc;
using BECOSOFT.ThirdParty;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Web.Binders;
using BECOSOFT.Web.Controllers;
using BECOSOFT.Web.Helpers;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace BECOSOFT.Web {
    public class MvcApplication : HttpApplication {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private static bool _reportUpdateRunning;
        public static string ApplicationVersion;

        protected void Application_Start() {
            Logger.Info("ServerGC enabled? {0}", System.Runtime.GCSettings.IsServerGC);
            // Do not add the X-Frame-Options header automaticaly. Header is set in web.config.
            AntiForgeryConfig.SuppressXFrameOptionsHeader = true;
            var jsonFactory = ValueProviderFactories.Factories.OfType<JsonValueProviderFactory>().FirstOrDefault();
            if (jsonFactory != null) {
                ValueProviderFactories.Factories.Remove(jsonFactory);
                ValueProviderFactories.Factories.Add(new MaxSizeJsonValueProviderFactory());
            }
            DoRegistrations();
            _ = CreateContainer();
            AddBinders();

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            ApplicationVersion = $"{version.Major:D2}.{version.Minor:D2}.{version.Build:D2}-{version.Revision}";
            Logger.Info("Finished {0}.", nameof(Application_Start));
        }
        protected void Application_Error() {
            var context = HttpContext.Current;
            var allErrors = context?.AllErrors;
            var exception = Server.GetLastError();
            if (context == null && exception == null) {
                Logger.Error("No information on request or last server error");
                return;
            }
            try {
                if (context == null) {
                    Logger.Error(exception);
                    return;
                }
                RequestContext requestContext;
                HttpRequest request;
                try {
                    request = context.Request;
                    requestContext = request.RequestContext;
                } catch (Exception e) {
                    Logger.Error(e, "Failed to get request context");
                    if (exception != null) {
                        Logger.Error(exception);
                    }
                    return;
                }
                var controllerName = (string)requestContext.RouteData.Values["controller"];
                var actionName = (string)requestContext.RouteData.Values["action"];
                if (exception == null) {
                    Logger.Error("Error in controller: {0}, action: {1}", controllerName, actionName);
                } else {
                    if (exception.IsErrorCode(16388)) {
                        // A potentially dangerous Request.Path value was detected from the client 
                        var pathAndQuery = request.Url.PathAndQuery.ToSplitList<string>("/");
                        if (pathAndQuery.Count > 1 && pathAndQuery[pathAndQuery.Count - 1].Contains("Password", StringComparison.InvariantCultureIgnoreCase)) {
                            var last = pathAndQuery[pathAndQuery.Count - 1];
                            var parts = last.ToSplitList<string>("&");
                            var newParts = new List<string>();
                            foreach (var part in parts) {
                                var splitPart = part.ToSplitList<string>("=");
                                if (splitPart.Count == 1 || !splitPart[0].Contains("Password", StringComparison.InvariantCultureIgnoreCase)) {
                                    newParts.Add(part);
                                } else {
                                    newParts.Add(splitPart[0] + $"={string.Join("=", splitPart.Skip(1).Select(s => "*****"))}");
                                }
                            }
                            pathAndQuery[pathAndQuery.Count - 1] = string.Join("&", newParts);
                        }
                        var url = new Uri(request.Url.GetLeftPart(UriPartial.Authority) + $"{string.Join("/", pathAndQuery)}");
                        Logger.Error(exception, "Invalid request url: {0}", url);

                        if (Debugger.IsAttached) {
                            return;
                        }

                        if (!(exception is HttpException)) {
                            return;
                        }
                        context.Response.Clear();
                        context.Server.ClearError();

                        var controller = DependencyResolver.Current.GetService<ErrorController>();
                        var route = new RouteData {
                            Values = {
                                ["controller"] = "Error",
                                ["action"] = "Index"
                            }
                        };
                        route.DataTokens.Add("GlobalAsax_ErrorMessage", Resources.Error_InvalidUrlError);
                        var wrapper = new HttpContextWrapper(Context);
                        var newRequestContext = new RequestContext(wrapper, route);
                        ((IController)controller).Execute(newRequestContext);
                        Context.ApplicationInstance.CompleteRequest();
                        return;
                    }
                    Logger.Error(exception, "Error in controller: {0}, action: {1}", controllerName, actionName);
                }
            } catch (Exception e) {
                Logger.Error(e);
                throw;
            } finally {
                if (allErrors.HasAny() && allErrors.Length > 1) {
                    Logger.Error("All other errors: ");
                    foreach (var error in allErrors.Skip(1)) {
                        Logger.Error(error);
                    }
                }
            }
        }

        protected void Application_BeginRequest(object sender, EventArgs e) {
            var origin = HttpContext.Current.Request.Headers["Origin"];
            if (origin != null) {
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", origin);
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET,POST");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "ApiKey");
            }

            if (HttpContext.Current.Request.HttpMethod == "OPTIONS") {
                HttpContext.Current.Response.End();
            }
        }

        protected void Application_PreSendRequestHeaders() {
            var httpContext = HttpContext.Current;
            if (httpContext != null) {
                var cookieValueSuffix = "; Secure; SameSite=none";

                var cookies = httpContext.Response.Cookies;
                for (var i = 0; i < cookies.Count; i++) {
                    var cookie = cookies[i];
                    cookie.Value += cookieValueSuffix;
                }
            }
        }

        private static void DoRegistrations() {
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            //GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        private static IContainer CreateContainer() {
            try {
                var executingAssembly = Assembly.GetExecutingAssembly();
                var assemblyName = executingAssembly.GetName();
                var websiteVersion = assemblyName.Version.ToString();
                Logger.Info("{0}, v{1}", assemblyName.Name, websiteVersion);
            } catch (Exception e) {
                Logger.Error(e, "Failed to retrieve version information for assembly");
            }
            var buildOptions = new ThirdPartyBuildOptions() {
                BuilderAction = BuilderAction,
            };
            var container = ThirdPartyModule.Build(buildOptions);
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            return container;

            void BuilderAction(ContainerBuilder builder) {
                builder.RegisterControllers(typeof(MvcApplication).Assembly).FindConstructorsWith(ThirdPartyModule.InternalConstructorFinder).PropertiesAutowired();
                builder.RegisterModule<AutofacWebTypesModule>();
            }
        }

        private static void AddBinders() {
            ModelBinders.Binders.Add(typeof(DateTime), new DateTimeBinder());
            ModelBinders.Binders.Add(typeof(DateTime?), new DateTimeBinder());
        }
    }
}