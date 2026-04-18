using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace BECOSOFT.Web.Extensions {
    public static class LabelForExtensions {
        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the specified expression.
        /// If the field is required, the "required" class is added.
        /// </summary>
        /// <returns>An HTML label element and the property name of the property that is represented by the expression.</returns>
        /// <param name="html">The HTML helper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the property to display.</param>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        public static MvcHtmlString FormLabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression) => FormLabelFor(html, expression, null, null);

        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the specified expression using the label text.
        /// If the field is required, the "required" class is added.
        /// </summary>
        /// <returns>An HTML label element and the property name of the property that is represented by the expression.</returns>
        /// <param name="html">The HTML helper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the property to display.</param>
        /// <param name="labelText">The label text to display.</param>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        public static MvcHtmlString FormLabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string labelText) => FormLabelFor(html, expression, labelText, null);

        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the specified expression.
        /// If the field is required, the "required" class is added.
        /// </summary>
        /// <returns>An HTML label element and the property name of the property that is represented by the expression.</returns>
        /// <param name="html">The HTML helper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the property to display.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The value.</typeparam>
        public static MvcHtmlString FormLabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes) => FormLabelFor(html, expression, null, htmlAttributes);

        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the specified expression.
        /// If the field is required, the "required" class is added.
        /// </summary>
        /// <returns>An HTML label element and the property name of the property that is represented by the expression.</returns>
        /// <param name="html">The HTML helper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the property to display.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        public static MvcHtmlString FormLabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IDictionary<string, object> htmlAttributes) => FormLabelFor(html, expression, null, htmlAttributes);

        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the specified expression.
        /// If the field is required, the "required" class is added.
        /// </summary>
        /// <returns>An HTML label element and the property name of the property that is represented by the expression.</returns>
        /// <param name="html">The HTML helper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the property to display.</param>
        /// <param name="labelText">The label text.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The Value.</typeparam>
        public static MvcHtmlString FormLabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string labelText, object htmlAttributes) => FormLabelFor(html, expression, labelText, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));

        /// <summary>
        /// Returns an HTML label element and the property name of the property that is represented by the specified expression.
        /// If the field is required, the "required" class is added.
        /// </summary>
        /// <returns>An HTML label element and the property name of the property that is represented by the expression.</returns>
        /// <param name="html">The HTML helper instance that this method extends.</param>
        /// <param name="expression">An expression that identifies the property to display.</param>
        /// <param name="labelText">The label text to display.</param>
        /// <param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        public static MvcHtmlString FormLabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string labelText, IDictionary<string, object> htmlAttributes) {
            if (!htmlAttributes.HasAny()) {
                htmlAttributes = new Dictionary<string, object>();
            }

            return html.LabelFor(expression, labelText, htmlAttributes);
        }
    }
}