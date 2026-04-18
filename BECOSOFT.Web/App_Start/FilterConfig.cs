using System.Web.Mvc;
using BECOSOFT.Web.Helpers;

namespace BECOSOFT.Web {
    public class FilterConfig {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
            filters.Add(new LogActionFilter());
        }
    }
}
