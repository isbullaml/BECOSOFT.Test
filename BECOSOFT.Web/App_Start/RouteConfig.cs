using BECOSOFT.Web.Helpers;
using System.Web.Mvc;
using System.Web.Routing;

namespace BECOSOFT.Web {
    public class RouteConfig {
        public static void RegisterRoutes(RouteCollection routes) {
            routes.MapMvcAttributeRoutes(new InheritableDirectRouteProvider());
            routes.RouteExistingFiles = true;

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
