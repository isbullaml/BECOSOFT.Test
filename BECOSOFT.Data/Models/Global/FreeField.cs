using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Helpers;
using System;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Models.Global {
    public abstract class FreeField : BaseEntity, IFreeField {
        public void SetFreeFieldValue<T>(FreeFieldType<T> type, int fieldIndex, T value) {
            var typeInfo = EntityConverter.GetEntityTypeInfo(GetType());
            var fieldName = GetFieldName(type, fieldIndex);
            var propertyInfo = typeInfo.GetPropertyInfo(fieldName, null);
            if (propertyInfo == null) {
                throw new ArgumentException(string.Format(Resources.PropertyType_InvalidArgument, fieldName, GetType().FullName));
            }
            propertyInfo.Setter(this, value);
        }

        public T GetFreeFieldValue<T>(FreeFieldType<T> type, int fieldIndex) {
            var typeInfo = EntityConverter.GetEntityTypeInfo(GetType());
            var fieldName = GetFieldName(type, fieldIndex);
            var propertyInfo = typeInfo.GetPropertyInfo(fieldName, null);
            if (propertyInfo == null) {
                throw new ArgumentException(string.Format(Resources.PropertyType_InvalidArgument, fieldName, GetType().FullName));
            }
            var converter = Converter.GetDelegate(propertyInfo.PropertyType);
            return (T) converter(propertyInfo.Getter(this));
        }

        private static string GetFieldName<T>(FreeFieldType<T> type, int fieldIndex) {
            string fieldName;
            if (type.UseDescription) {
                var prefix = type.ColumnType.GetDescription();
                fieldName = string.Format(prefix, fieldIndex);
            } else {
                var prefix = type.ColumnType.ToString();
                fieldName = prefix + fieldIndex;
            }

            return fieldName;
        }

        public static Expression<Func<TEntity, object>> GetPropertyExpression<T, TEntity>(FreeFieldType<T> type, int fieldIndex) where TEntity : FreeField {
            var typeInfo = EntityConverter.GetEntityTypeInfo(typeof(TEntity));
            var fieldName = GetFieldName(type, fieldIndex);
            var propertyInfo = typeInfo.GetPropertyInfo(fieldName, null);
            return propertyInfo.CreatePropertyExpression<TEntity>();
        }
    }
}
