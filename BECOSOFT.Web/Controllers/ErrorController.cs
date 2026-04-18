using System;
using System.Web.Mvc;

namespace BECOSOFT.Web.Controllers {
    [RoutePrefix("")]
    public class ErrorController : Controller {
        private static readonly Random Random = new Random();

        // source: https://gist.github.com/NickCraver/c9458f2e007e9df2bdf03f8a02af1d13
        private static readonly string[] TenHoursOfFun = {
            "https://www.youtube.com/watch?v=wbby9coDRCk",
            "https://www.youtube.com/watch?v=nb2evY0kmpQ",
            "https://www.youtube.com/watch?v=eh7lp9umG2I",
            "https://www.youtube.com/watch?v=z9Uz1icjwrM",
            "https://www.youtube.com/watch?v=Sagg08DrO5U",
            "https://www.youtube.com/watch?v=ER97mPHhgtM",
            "https://www.youtube.com/watch?v=jI-kpVh6e1U",
            "https://www.youtube.com/watch?v=jScuYd3_xdQ",
            "https://www.youtube.com/watch?v=S5PvBzDlZGs",
            "https://www.youtube.com/watch?v=9UZbGgXvCCA",
            "https://www.youtube.com/watch?v=O-dNDXUt1fg",
            "https://www.youtube.com/watch?v=MJ5JEhDy8nE",
            "https://www.youtube.com/watch?v=VnnWp_akOrE",
            "https://www.youtube.com/watch?v=jwGfwbsF4c4",
            "https://www.youtube.com/watch?v=8ZcmTl_1ER8",
            "https://www.youtube.com/watch?v=gLmcGkvJ-e0",
            "https://www.youtube.com/watch?v=ozPPwl53c_4",
            "https://www.youtube.com/watch?v=KMFOVSWn0mI",
            "https://www.youtube.com/watch?v=clU0Sh9ngmY",
            "https://www.youtube.com/watch?v=sCNrK-n68CM",
        };

        [Route("admin.php")]
        [Route("admin/login.php")]
        [Route("administrator/index.php")]
        [Route("ajaxproxy/proxy.php")]
        [Route("bitrix/admin/index.php")]
        [Route("index.php")]
        [Route("magmi/web/magmi.php")]
        [Route("wp-admin/admin-ajax.php")]
        [Route("wp-admin/includes/themes.php")]
        [Route("wp-admin/options-link.php")]
        [Route("wp-admin/post-new.php")]
        [Route("wp-admin/style.php")]
        [Route("wp-login.php")]
        [Route("xmlrpc.php")]
        [Route("wordpress")]
        [Route(".git/HEAD")]
        [Route("data/adminer.php")]
        [Route("manager/adminer.php")]
        [Route("phpmyadmin.php")]
        [Route("style.php")]
        [Route("wp-cc.php")]
        [Route("wp-commentin.php")]
        [Route("public/_ignition/health-check")]
        [Route("_ignition/health-check")]
        public ActionResult No() {
            return Redirect(TenHoursOfFun[Random.Next(0, TenHoursOfFun.Length)]);
        }

        public ActionResult Index() {
            if (!(TempData["GlobalAsax_ErrorMessage"] is string errorMessage)) {
                object temp = null;
                RouteData?.DataTokens?.TryGetValue("GlobalAsax_ErrorMessage", out temp);
                errorMessage = temp as string;
            }
            ViewBag.ErrorMessage = errorMessage;
            return View("ErrorWithoutLayout");
        }

        public ViewResult NotFound() {
            Response.StatusCode = 404; //you may want to set this to 200
            return View("NotFoundWithoutLayout");
        }

        [Route("404.aspx")]
        public ViewResult NotFoundForAspx() {
            Response.StatusCode = 404; //you may want to set this to 200
            return View("NotFoundWithoutLayout");
        }
    }
}