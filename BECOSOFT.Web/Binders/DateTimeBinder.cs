using BECOSOFT.Utilities.Helpers;
using System.Web.Mvc;

namespace BECOSOFT.Web.Binders {
    public class DateTimeBinder : IModelBinder {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext) {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (value == null) { return null; }
            return DateTimeHelpers.Parse(value.AttemptedValue);
        }
    }
}