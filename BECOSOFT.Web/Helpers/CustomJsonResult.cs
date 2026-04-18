using BECOSOFT.Utilities.Extensions;
using Newtonsoft.Json;
using System;
using System.Web.Mvc;

namespace BECOSOFT.Web.Helpers {
    public class CustomJsonResult : JsonResult {
        public override void ExecuteResult(ControllerContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }
            if (JsonRequestBehavior == JsonRequestBehavior.DenyGet &&
                string.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase)) {
                throw new InvalidOperationException("GET not allowed");
            }

            var response = context.HttpContext.Response;

            response.ContentType = ContentType?.NullIf("") ?? "application/json";
            if (ContentEncoding != null) {
                response.ContentEncoding = ContentEncoding;
            }
            if (Data == null) {
                return;
            }
            var serialized = JsonConvert.SerializeObject(Data);
            response.Write(serialized);
        }
    }
}