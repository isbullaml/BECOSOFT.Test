using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Validation.Attributes;
using BECOSOFT.Utilities.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BECOSOFT.Data.Converters {
    /// <summary>
    /// Class containing all info about an entityproperty
    /// </summary>
    [DebuggerDisplay("Prop: {PropertyName}, Col: {ColumnName}, Base: {BaseEntityType}")]
    public class EntityPropertyInfo : IEquatable<EntityPropertyInfo> {
        private PropertyInfo _propertyInfo;
        public EntityTypeInfo Parent { get; }
        public string PropertyName { get; }
        /// <summary>
        /// The name of the column of the property
        /// </summary>
        public string ColumnName { get; }
        public string EscapedColumnName { get; }
        /// <summary>
        /// The lowercase name of the column of the property
        /// </summary>
        public string LowerCaseColumnName { get; }

        /// <summary>
        /// The propertyinfo
        /// </summary>
        public PropertyInfo PropertyInfo {
            get { return _propertyInfo; }
            set {
                _propertyInfo = value;
                PropertyType = _propertyInfo.PropertyType;
                BaseEntityType = RetrieveBaseEntityType();
                ValidationAttribute = _propertyInfo.GetCustomAttributes<Attribute>().OfType<IValidationAttribute>().FirstOrDefault();
            }
        }

        /// <summary>
        /// Value indicating whether the property is insertable
        /// </summary>
        public bool Insertable { get; set; }
        /// <summary>
        /// Value indicating whether the property is updateable
        /// </summary>
        public bool Updateable { get; set; }
        /// <summary>
        /// The fieldtype of the property
        /// </summary>
        public FieldType DatabaseType { get; set; }

        /// <summary>
        /// The setter of the property
        /// </summary>
        public Action<object, object> Setter { get; set; }

        /// <summary>
        /// The getter of the property
        /// </summary>
        public Func<object, object> Getter { get; set; }
        /// <summary>
        /// Value indicating whether the property is the primary key
        /// </summary>
        public bool IsPrimaryKey { get; }
        /// <summary>
        /// Value indicating whether the property is a linked entity
        /// </summary>
        public bool IsLinkedEntity { get; set; }
        /// <summary>
        /// Value indicating whether the property is a linked base result
        /// </summary>
        public bool IsLinkedBaseResult { get; set; }
        /// <summary>
        /// Value indicating whether the property are linked base results
        /// </summary>
        public bool AreLinkedBaseResults { get; set; }
        /// <summary>
        /// Value indicating whether the property is an inverse linked entity
        /// </summary>
        public bool IsInverseLinkedEntity { get; set; }
        /// <summary>
        /// Value indicating whether the property is a base child
        /// </summary>
        public bool IsBaseChild { get; set; }
        /// <summary>
        /// Value indicating whether the property are linked entities
        /// </summary>
        public bool AreLinkedEntities { get; set; }
        /// <summary>
        /// The primary key column of the property (to use in the own table in joins)
        /// </summary>
        public string PrimaryKeyColumn { get; set; }
        /// <summary>
        /// The foreign key column of the property (to use in the joined table in joins)
        /// </summary>
        public string ForeignKeyColumn { get; set; }
        /// <summary>
        /// Value indicating whether the property is searchable
        /// </summary>
        public bool IsSearchable { get; set; }
        /// <summary>
        /// Value indicating whether the property is abstract
        /// </summary>
        public bool IsAbstract { get; set; }
        /// <summary>
        /// Value indicating whether the property is a filter
        /// </summary>
        public bool IsFilter { get; set; }
        /// <summary>
        /// Indicates if the property is linked in any way
        /// </summary>
        public bool IsLinked => AreLinkedEntities || IsLinkedEntity || IsInverseLinkedEntity || IsLinkedBaseResult || AreLinkedBaseResults;

        /// <summary>
        /// Indicates that the <see cref="ColumnName"/> contains a format specifier.
        /// </summary>
        public bool HasFormatSpecifier { get; }

        public IValidationAttribute ValidationAttribute { get; private set; }

        public EntityPropertyInfo(EntityTypeInfo parent, string propName, string colName, bool isPrimaryKey) {
            Parent = parent;
            PropertyName = propName;
            ColumnName = colName;
            EscapedColumnName = colName.Replace("-", "_");
            HasFormatSpecifier = colName.Contains("{0}");
            LowerCaseColumnName = ColumnName?.ToLowerInvariant();
            IsPrimaryKey = isPrimaryKey;
        }

        /// <summary>
        /// Value indicating if it is a property
        /// </summary>
        /// <returns></returns>
        public bool IsProperty() {
            return PropertyInfo != null;
        }

        /// <summary>
        /// The type of the property
        /// </summary>
        public Type PropertyType { get; private set; }

        /// <summary>
        /// The type of the base entity
        /// </summary>
        public Type BaseEntityType { get; private set; }

        private Type RetrieveBaseEntityType() {
            var propType = PropertyType;
            if (!IsProperty()) {
                return propType;
            }

            var genericListParameter = propType.GetGenericArguments().FirstOrDefault();
            if (genericListParameter != null && propType.IsGenericList() && genericListParameter.IsInterfaceImplementationOf<IEntity>()) {
                return genericListParameter;
            }
            return propType;
        }

        public Expression<Func<T, object>> CreatePropertyExpression<T>() {
            var type = typeof(T);
            if (type != Parent.EntityType) {
                throw new ArgumentException("Invalid type", nameof(T));
            }
            var instance = Expression.Parameter(type);
            var expr = Expression.MakeMemberAccess(instance, PropertyInfo);
            if (PropertyType == typeof(string) || PropertyType.IsClass) {
                return Expression.Lambda<Func<T, object>>(expr, instance);
            }
            var convExpr = Expression.Convert(expr, typeof(object));
            return Expression.Lambda<Func<T, object>>(convExpr, instance);
        }

        public bool Equals(EntityPropertyInfo other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(_propertyInfo, other._propertyInfo) && Equals(Parent, other.Parent);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((EntityPropertyInfo) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (_propertyInfo != null ? _propertyInfo.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Parent != null ? Parent.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}