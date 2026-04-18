using BECOSOFT.Utilities.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Mvc.Routing;

namespace BECOSOFT.Web.Helpers {
    public class InheritableDirectRouteProvider : DefaultDirectRouteProvider {
        //See: https://stackoverflow.com/a/39625735
        protected override IReadOnlyList<IDirectRouteFactory> GetControllerRouteFactories(ControllerDescriptor controllerDescriptor) {
            // Inherit route attributes decorated on base class controller
            // GOTCHA: RoutePrefixAttribute doesn't show up here, even though we were expecting it to.
            //  Am keeping this here anyways, but am implementing an ugly fix by overriding GetRoutePrefix
            return GetCustomAttributes<IDirectRouteFactory>(controllerDescriptor, true);
        }

        protected override IReadOnlyList<IDirectRouteFactory> GetActionRouteFactories(ActionDescriptor actionDescriptor) {
            // Inherit route attributes decorated on base class controller's actions
            return GetCustomAttributes<IDirectRouteFactory>(actionDescriptor, true);
        }

        protected override string GetRoutePrefix(ControllerDescriptor controllerDescriptor) {
            // Get the calling controller's route prefix
            var routePrefix = base.GetRoutePrefix(controllerDescriptor);

            // Iterate through each of the calling controller's base classes that inherit from HttpController
            var baseControllerType = controllerDescriptor.ControllerType.BaseType;
            var controllerInterface = typeof(IController);

            while (baseControllerType != null && controllerInterface.IsAssignableFrom(baseControllerType)) {
                // Get the base controller's route prefix, if it exists
                // GOTCHA: There are two RoutePrefixAttributes... System.Web.Http.RoutePrefixAttribute and System.Web.Mvc.RoutePrefixAttribute!
                //  Depending on your controller implementation, either one or the other might be used... checking against typeof(RoutePrefixAttribute) 
                //  without identifying which one will sometimes succeed, sometimes fail.
                //  Since this implementation is generic, I'm handling both cases.  Preference would be to extend System.Web.Mvc and System.Web.Http
                var baseRoutePrefix = baseControllerType.GetCustomAttribute<RoutePrefixAttribute>();
                if (baseRoutePrefix != null) {
                    // A trailing slash is added by the system. Only add it if we're prefixing an existing string
                    var trailingSlash = routePrefix.IsNullOrEmpty() ? "" : "/";
                    // Prepend the base controller's prefix
                    routePrefix = baseRoutePrefix.Prefix + trailingSlash + routePrefix;
                }

                // Traverse up the base hierarchy to check for all inherited prefixes
                baseControllerType = baseControllerType.BaseType;
            }

            return routePrefix;
        }

        private IReadOnlyList<T> GetCustomAttributes<T>(ICustomAttributeProvider model, bool inherit) {
            return model.GetCustomAttributes(typeof(T), inherit).Select(a => (T) a).ToList();
        }
    }
}