using BECOSOFT.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace BECOSOFT.Web.Extensions {
    public static class HtmlHelperExtensions {
        public static MvcHtmlString Tooltip(this HtmlHelper helper, string text) {
            var tooltip =
                $@"
                    <span class='bcs-tooltip' data-toggle='tooltip' title='{text}'>
                        <i class='fa-circle-question fa-light' aria-hidden='true'></i>
                    </span>
                ";
            return new MvcHtmlString(tooltip);
        }

        public static MvcHtmlString FormRowFor<TModel, TProp>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProp>> expression,
                                                              string labelString, string extraClasses = null, bool isDisabled = false) {
            if (labelString.IsNullOrWhiteSpace()) {
                var metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
                labelString = metadata.DisplayName;
            }
            var label = helper.FormLabelFor(expression, labelString, new { @class = "w-100 mb-0 text-truncate" });

            MvcHtmlString editor;
            if (isDisabled) {
                editor = helper.EditorFor(expression, new { htmlAttributes = new { @class = $"form-control {extraClasses}".Trim(), disabled = "disabled" } });
            } else {
                editor = helper.EditorFor(expression, new { htmlAttributes = new { @class = $"form-control {extraClasses}".Trim() } });
            }
            var validationMessage = helper.ValidationMessageFor(expression, string.Empty, new { @class = "text-danger font-italic" });
            var result = $@"
                <div class='row form-row'>
                    <div class='col-12'>
                        {label}
                        {editor}
                        {validationMessage}
                    </div>
                </div>
                ";
            return new MvcHtmlString(result);
        }

        public static MvcHtmlString DisplayRowFor<TModel, TProp>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProp>> expression, bool ignoreIfEmpty = false) {
            var metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            var labelString = metadata.DisplayName;
            return DisplayRowFor(helper, expression, labelString, ignoreIfEmpty);
        }

        public static MvcHtmlString DisplayRowFor<TModel, TProp>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProp>> expression,
                                                                 string labelString, bool ignoreIfEmpty = false) {
            var label = helper.LabelFor(expression, labelString, new { @class = "bcs-font-weight-semibold mb-0" });
            var display = helper.DisplayFor(expression);
            if (ignoreIfEmpty) {
                var displayValue = expression.Compile()(helper.ViewData.Model);
                if (displayValue == null || displayValue.Equals(default(TProp)) || displayValue.ToString().IsNullOrWhiteSpace()) {
                    return MvcHtmlString.Empty;
                }
            }
            var result = $@"
                <div class='row form-row mb-2'>
                    <div class='col-12'>
                        {label}
                        <br/>
                        {display}
                    </div>
                </div>
                ";
            return new MvcHtmlString(result);
        }
        /// <summary>
        /// A checkbox (with or without label).
        /// </summary>
        public static MvcHtmlString CustomCheckboxFor<TModel>(this HtmlHelper<TModel> helper, Expression<Func<TModel, bool>> expression, bool isDisabled = false, string labelString = "") {
            MvcHtmlString checkbox;
            if (isDisabled) {
                checkbox = helper.CheckBoxFor(expression, new { @class = "custom-control-input", disabled = "disabled" });
            } else {
                checkbox = helper.CheckBoxFor(expression, new { @class = "custom-control-input" });
            }
            MvcHtmlString label;
            if (labelString.IsNullOrWhiteSpace()) {
                label = helper.FormLabelFor(expression, " ", new { @class = "custom-control-label" });
            } else {
                label = helper.FormLabelFor(expression, labelString, new { @class = "custom-control-label" });
            }
            var result =
                $@"
                    <div class='custom-control custom-checkbox {(labelString.IsNullOrWhiteSpace() ? "bcs-checkbox-placement-correction" : "")}'>
                        {checkbox}
                        {label}
                    </div>
                ";
            return new MvcHtmlString(result);
        }

        /// <summary>
        /// An integer number input with increase/decrease buttons.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="helper"></param>
        /// <param name="expression">The property to reference.</param>
        /// <param name="name">The name that to connect the buttons to the input field.</param>
        /// <param name="min">The minimum input value.</param>
        /// <param name="max">The maximum input value.</param>
        /// <param name="extraClasses">Extra CSS classes.</param>
        /// <param name="isDisabled">Whether the input is disabled.</param>
        /// <param name="htmlAttributes">An object with extra attributes for the input</param>
        /// <returns></returns>
        public static MvcHtmlString CustomNumberInput<TModel>(this HtmlHelper<TModel> helper, Expression<Func<TModel, int>> expression, string name, int min, int max, string extraClasses = "", bool isDisabled = false, object htmlAttributes = null) {
            var classes = "form-control input-number text-right " + extraClasses;
            var disabledButton = "";

            if (htmlAttributes == null) { htmlAttributes = new object(); }


            var allHtmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            allHtmlAttributes.Add("data-custom-number-name", name);

            if (isDisabled) {
                if (!allHtmlAttributes.ContainsKey("disabeld")) {
                    allHtmlAttributes.Add("disabled", "disabled");
                }
                disabledButton = "disabled";
            }

            if (allHtmlAttributes.TryGetValue("class", out var currentClass)) {
                allHtmlAttributes["class"] = currentClass + " " + classes;
            } else {
                allHtmlAttributes.Add("class", classes);
            }

            allHtmlAttributes["min"] = min;
            allHtmlAttributes["max"] = max;
            allHtmlAttributes["Name"] = name;
            allHtmlAttributes["data-custom-number-name"]  = name;

            var result =
                $@"
                    <div class='input-group' data-custom-number-input>
                        {helper.TextBoxFor(expression, htmlAttributes: allHtmlAttributes)}
                        <span class='input-group-btn'>
                            <button type='button' {disabledButton} tabindex=""-1"" class='btn btn-sm btn-number' data-custom-number-button-type='plus' data-custom-number-name={name}>
                                <i class=""fa-caret-up fa-solid fa-sm"" aria-hidden=""true""></i>
                            </button>
                        </span>
                        <span class='input-group-btn'>
                            <button type='button' {disabledButton} tabindex=""-1"" class='btn btn-sm btn-number' data-custom-number-button-type='minus' data-custom-number-name={name}>
                                <i class=""fa-caret-down fa-solid fa-sm"" aria-hidden=""true""></i>
                            </button>
                        </span>
                    </div>
                ";
            return new MvcHtmlString(result);
        }

        /// <summary>
        /// A display label and disabled checkbox for use in Details/Delete pages.
        /// </summary>
        public static MvcHtmlString CustomDisplayCheckboxFor<TModel>(this HtmlHelper<TModel> helper, Expression<Func<TModel, bool>> expression, string labelString) {
            var label = helper.LabelFor(expression, labelString, new { @class = "bcs-font-weight-semibold mb-0" });
            var checkbox = helper.CustomCheckboxFor(expression, true);
            var result =
                $@"
                    {label}
                    <div class='mb-2'>
                        {checkbox}
                    </div>
                ";
            return new MvcHtmlString(result);
        }

        /// <summary>
        /// A switch.
        /// </summary>
        public static MvcHtmlString CustomSwitchFor<TModel>(this HtmlHelper<TModel> helper, Expression<Func<TModel, bool>> expression, bool isDisabled = false, string labelString = "") {
            MvcHtmlString checkbox;
            if (isDisabled) {
                checkbox = helper.CheckBoxFor(expression, new { @class = $"custom-control-input", disabled = "disabled" });
            } else {
                checkbox = helper.CheckBoxFor(expression, new { @class = $"custom-control-input" });
            }
            MvcHtmlString label;
            if (labelString.IsNullOrWhiteSpace()) {
                label = helper.FormLabelFor(expression, " ", new { @class = $"custom-control-label" });
            } else {
                label = helper.FormLabelFor(expression, labelString, new { @class = $"custom-control-label" });
            }
            var result =
                $@"
                    <div class='custom-control custom-switch'>
                        {checkbox}
                        {label}
                    </div>
                ";
            return new MvcHtmlString(result);
        }

        public static MvcHtmlString ValidationMessagesFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null) {
            var propertyName = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData).PropertyName;
            propertyName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(propertyName);
            var modelState = htmlHelper.ViewData.ModelState;

            if (!modelState.ContainsKey(propertyName) || modelState[propertyName].Errors.Count <= 1) {
                // Revert to default behaviour.
                return htmlHelper.ValidationMessageFor(expression, null, htmlAttributes as IDictionary<string, object> ?? htmlAttributes);
            }
            // If we have multiple (server-side) validation errors, collect and present them.
            var msgs = new StringBuilder();
            foreach (var error in modelState[propertyName].Errors) {
                msgs.Append("<span>").Append(error.ErrorMessage).AppendLine("</span><br/>");
            }

            // Return standard ValidationMessageFor, overriding the message with our concatenated list of messages.
            var msgSpan = htmlHelper.ValidationMessageFor(expression, "{0}", htmlAttributes as IDictionary<string, object> ?? htmlAttributes);
            var msgDiv = msgSpan.ToHtmlString().Replace("<span ", "<div ").Replace("</span>", "</div>");
            return new MvcHtmlString(msgDiv.FormatWith(msgs));

        }


    }
}