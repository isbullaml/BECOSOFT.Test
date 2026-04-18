using BECOSOFT.Utilities.Extensions;
using System;

namespace BECOSOFT.Utilities.Models {
    /// <summary>
    /// Class containing info about the type
    /// </summary>
    public class TypeInformation {
        /// <summary>
        /// The type
        /// </summary>
        public Type Type { get; }
        /// <summary>
        /// Value indicating whether the type is generic
        /// </summary>
        public bool IsGenericType { get; }
        /// <summary>
        /// Value indicating whether the type can be NULL
        /// </summary>
        public bool IsNullableOf { get; }
        /// <summary>
        /// The type of an object
        /// </summary>
        public TypeCode TypeCode { get; }
        /// <summary>
        /// Value indicating whether the type is a decimal
        /// </summary>
        public bool IsDecimal { get; }
        /// <summary>
        /// Value indicating whether the type is an integer
        /// </summary>
        public bool IsInteger { get; }
        /// <summary>
        /// Value indicating whether the type is a numeric
        /// </summary>
        public bool IsNumeric { get; }
        /// <summary>
        /// Value indicating whether the type is <see cref="string"/>
        /// </summary>
        public bool IsString { get; }
        /// <summary>
        /// Value indicating whether the type is <see cref="DateTime"/>
        /// </summary>
        public bool IsDateTime { get; }
        /// <summary>
        /// The underlying type
        /// </summary>
        public Type UnderlyingType { get; }
        /// <summary>
        /// Value indicating whether the type is an enum
        /// </summary>
        public bool IsEnum { get; }
        public bool IsEnumWithFlag { get; }
        public bool IsPrimitive { get; set; }
        public Type BaseType { get; }

        public TypeInformation(Type type) {
            Type = type;
            IsGenericType = type.IsGenericType;
            IsNullableOf = type.IsNullableType();
            TypeCode = Type.GetTypeCode(IsNullableOf ? Nullable.GetUnderlyingType(type) : type);
            IsNumeric = type.IsNumeric();
            IsInteger = type.IsInteger();
            IsDecimal = type.IsDecimal();
            IsString = type == typeof(string);
            IsDateTime = type == typeof(DateTime);
            IsPrimitive = type.IsPrimitive;
            IsEnum = type.IsEnum;
            if (IsEnum) {
                IsEnumWithFlag = Type.IsDefined(typeof(FlagsAttribute), false);
            }
            if (IsGenericType && IsNullableOf) {
                UnderlyingType = Nullable.GetUnderlyingType(type);
            } else if (IsEnum) {
                UnderlyingType = type.GetEnumUnderlyingType();
            }
            BaseType = UnderlyingType ?? Type;
        }
    }
}
