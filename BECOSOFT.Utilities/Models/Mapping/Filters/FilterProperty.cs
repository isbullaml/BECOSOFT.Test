using BECOSOFT.Utilities.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq.Expressions;

namespace BECOSOFT.Utilities.Models.Mapping.Filters {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FilterProperty {
        /// <summary>
        /// Actual parent class name of the property (<see cref="FilterOption.Entity"/> on <see cref="FilterOption"/> is used for custom grouping)
        /// </summary>
        [JsonIgnore]
        public string ParentClass { get; set; }

        public string Property { get; set; }

        [JsonIgnore]
        public Func<string> CustomQueryPart { get; set; }

        [JsonIgnore]
        public string ActualProperty { get; set; }

        [JsonIgnore]
        public string PreselectionFilterProperty { get; set; }

        public string FriendlyName { get; set; }

        public string SearchAction { get; set; }

        [JsonIgnore]
        public FilterDataType DataTypeValue { get; set; }

        public string DataType {
            get => DataTypeValue.ToString().ToLower();
            set => DataTypeValue = value.To<FilterDataType>();
        }

        public List<FilterBasePossibleValue> PossibleValues { get; set; }

        public List<FilterPossiblePropertyLabel> PossibleProperties { get; set; }

        private FilterProperty(FilterDataType dataType) {
            DataTypeValue = dataType;
        }

        public static FilterProperty Create<T>(FilterDataType dataType, Expression<Func<T, object>> propertyExpression, string friendlyName) {
            var prop = new FilterProperty(dataType) {
                FriendlyName = friendlyName,
            };
            prop.ParseExpression(propertyExpression);
            return prop;
        }

        private void ParseExpression<T>(Expression<Func<T, object>> expression) {
            Expression e;
            if (expression.Body is UnaryExpression ce) {
                e = ce.Operand;
            } else {
                e = expression.Body;
            }
            if (e is MemberExpression p) {
                Property = p.Member.Name;
            } else {
                Property = typeof(T).Name;
            }
            if (DataTypeValue == FilterDataType.StringStrippedFromHtml) {
                Property = $"{Property}_HtmlStripped";
            } else if (DataTypeValue == FilterDataType.StringStrippedFromHtmlLength) {
                Property = $"{Property}_HtmlStrippedLength";
            }
            ParentClass = typeof(T).AssemblyQualifiedName;
        }

        public static FilterProperty Integer<T>(Expression<Func<T, object>> propertyExpression, string name) {
            return Create(FilterDataType.Integer, propertyExpression, name);
        }

        public static FilterProperty String<T>(Expression<Func<T, object>> propertyExpression, string name) {
            return Create(FilterDataType.String, propertyExpression, name);
        }

        public static FilterProperty StringStrippedFromHtml<T>(Expression<Func<T, object>> propertyExpression, string name) {
            return Create(FilterDataType.StringStrippedFromHtml, propertyExpression, name);
        }

        public static FilterProperty StringStrippedFromHtmlLength<T>(Expression<Func<T, object>> propertyExpression, string name) {
            return Create(FilterDataType.StringStrippedFromHtmlLength, propertyExpression, name);
        }

        public static FilterProperty Boolean<T>(Expression<Func<T, object>> propertyExpression, string name) {
            return Create(FilterDataType.Boolean, propertyExpression, name);
        }

        public static FilterProperty Decimal<T>(Expression<Func<T, object>> propertyExpression, string name) {
            return Create(FilterDataType.Decimal, propertyExpression, name);
        }

        public static FilterProperty Date<T>(Expression<Func<T, object>> propertyExpression, string name) {
            return Create(FilterDataType.Date, propertyExpression, name);
        }

        public static FilterProperty Select<T>(Expression<Func<T, object>> propertyExpression, string name) {
            return Create(FilterDataType.Select, propertyExpression, name);
        }

        public static FilterProperty LinkedSelect<T>(Expression<Func<T, object>> propertyExpression, string name) {
            var prop = Create(FilterDataType.LinkedSelect, propertyExpression, name);
            prop.PossibleProperties = new List<FilterPossiblePropertyLabel>();
            prop.PossibleValues = new List<FilterBasePossibleValue>();
            return prop;
        }

        public static FilterProperty Search<T>(Expression<Func<T, object>> propertyExpression, string name, string searchAction) {
            var prop = Create(FilterDataType.Search, propertyExpression, name);
            prop.SearchAction = searchAction;
            return prop;
        }

        public static FilterProperty Exists<T>(Expression<Func<T, object>> propertyExpression, string name) {
            return Create(FilterDataType.Exists, propertyExpression, name);
        }

        private string DebuggerDisplay => $"{Property} ({FriendlyName}), {DataType}";
    }
}