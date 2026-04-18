using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Numeric;
using BECOSOFT.Utilities.Helpers;
using BECOSOFT.Utilities.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace BECOSOFT.Utilities.Converters {
    /// <summary>
    /// Class for converting types
    /// </summary>
    public static class Converter {
        private delegate bool TryParseStringFunc<TU>(string input, out TU output);

        private static readonly ConcurrentDictionary<Type, Func<object, object>> GenericMethodData =
            new ConcurrentDictionary<Type, Func<object, object>>();

        /// <summary>
        /// Get the delegate for converting
        /// </summary>
        /// <param name="targetType">The targeted type</param>
        /// <returns>A func which contains the conversion</returns>
        public static Func<object, object> GetDelegate(Type targetType) {
            Func<object, object> methodDelegate;
            if (!GenericMethodData.TryGetValue(targetType, out methodDelegate)) {
                var method = typeof(Converter).GetMethod("GetValue", new[] { typeof(object) });
                var methodInfo = method.MakeGenericMethod(targetType);
                methodDelegate = BuildAccessor(methodInfo);
                GenericMethodData.TryAdd(targetType, methodDelegate);
            }
            return methodDelegate;
        }

        private static Func<object, object> BuildAccessor(MethodInfo method) {
            var objParameter = Expression.Parameter(typeof(object), "objParameter");
            return
                Expression.Lambda<Func<object, object>>(
                              Expression.Convert(
                                  Expression.Call(null, method, objParameter),
                                  typeof(object)),
                              objParameter)
                          .Compile();
        }

        public static T GetValue<T>(bool value) {
            return HandleBooleanValue<T>(value, TypeInformationRetriever<T>.Instance());
        }

        public static T GetValue<T>(char value) {
            return HandleCharValue<T>(value, TypeInformationRetriever<T>.Instance());
        }

        public static T GetValue<T>(byte value) {
            return HandleByteValue<T>(value, TypeInformationRetriever<T>.Instance());
        }

        public static T GetValue<T>(sbyte value) {
            return HandleSByteValue<T>(value, TypeInformationRetriever<T>.Instance());
        }

        public static T GetValue<T>(short value) {
            return HandleInt16Value<T>(value, TypeInformationRetriever<T>.Instance());
        }

        public static T GetValue<T>(ushort value) {
            return HandleUInt16Value<T>(value, TypeInformationRetriever<T>.Instance());
        }

        public static T GetValue<T>(int value) {
            return HandleInt32Value<T>(value, TypeInformationRetriever<T>.Instance());
        }

        public static T GetValue<T>(uint value) {
            return HandleUInt32Value<T>(value, TypeInformationRetriever<T>.Instance());
        }

        public static T GetValue<T>(long value) {
            return HandleInt64Value<T>(value, TypeInformationRetriever<T>.Instance());
        }

        public static T GetValue<T>(ulong value) {
            return HandleUInt64Value<T>(value, TypeInformationRetriever<T>.Instance());
        }

        public static T GetValue<T>(float value) {
            return HandleSingleValue<T>(value, TypeInformationRetriever<T>.Instance());
        }

        public static T GetValue<T>(double value) {
            return HandleDoubleValue<T>(value, TypeInformationRetriever<T>.Instance());
        }

        public static T GetValue<T>(decimal value) {
            return HandleDecimalValue<T>(value, TypeInformationRetriever<T>.Instance());
        }

        public static T GetValue<T>(DateTime value) {
            return HandleDateTimeValue<T>(value, TypeInformationRetriever<T>.Instance());
        }

        public static T GetValue<T>(string value) {
            return HandleStringValue<T>(value, TypeInformationRetriever<T>.Instance());
        }

        public static T GetValue<T>(Guid value) {
            return HandleGuidValue<T>(value, TypeInformationRetriever<T>.Instance());
        }

        private static T HandleGuidValue<T>(Guid value, TypeInformation targetTypeInformation) {
            if (targetTypeInformation.TypeCode == TypeCode.String) {
                return BoxingSafeConverter<string, T>.Instance(value.ToString());
            }

            return TypeActivator<T>.Instance();
        }

        public static T GetValue<T>(object value) {
            if (value == null) {
                return TypeActivator<T>.Instance();
            }

            var targetTypeInformation = TypeInformationRetriever<T>.Instance();
            var targetType = targetTypeInformation.Type;

            if (targetType == typeof(string)) {
                return BoxingSafeConverter<string, T>.Instance(value.ToString());
            }

            if (targetType == typeof(DBNull)) {
                return TypeActivator<T>.Instance();
            }

            var valueType = value.GetType();
            if (valueType == targetType) {
                return (T)value;
            }

            var valueTypeCode = GetTypeCode(valueType);
            //var valueTypeCode = valueType.GetTypeInformation().TypeCode;
            if (targetTypeInformation.IsEnum) {
                return HandleObjectToEnum<T>(value, valueTypeCode, targetTypeInformation);
            }

            switch (valueTypeCode) {
                case TypeCode.Empty:
                case TypeCode.Object:
                    if (targetType.GetNonNullableType() == typeof(TimeSpan)) {
                        return HandleTimeSpanValue<T>((TimeSpan)value, targetTypeInformation);
                    }
                    break;
                case TypeCode.DBNull:
                    return TypeActivator<T>.Instance();
                case TypeCode.Boolean:
                    return HandleBooleanValue<T>((bool)value, targetTypeInformation);
                case TypeCode.Char:
                    return HandleCharValue<T>((char)value, targetTypeInformation);
                case TypeCode.SByte:
                    return HandleSByteValue<T>((sbyte)value, targetTypeInformation);
                case TypeCode.Byte:
                    return HandleByteValue<T>((byte)value, targetTypeInformation);
                case TypeCode.Int16:
                    return HandleInt16Value<T>((short)value, targetTypeInformation);
                case TypeCode.UInt16:
                    return HandleUInt16Value<T>((ushort)value, targetTypeInformation);
                case TypeCode.Int32:
                    return HandleInt32Value<T>((int)value, targetTypeInformation);
                case TypeCode.UInt32:
                    return HandleUInt32Value<T>((uint)value, targetTypeInformation);
                case TypeCode.Int64:
                    return HandleInt64Value<T>((long)value, targetTypeInformation);
                case TypeCode.UInt64:
                    return HandleUInt64Value<T>((ulong)value, targetTypeInformation);
                case TypeCode.Single:
                    return HandleSingleValue<T>((float)value, targetTypeInformation);
                case TypeCode.Double:
                    return HandleDoubleValue<T>((double)value, targetTypeInformation);
                case TypeCode.Decimal:
                    return HandleDecimalValue<T>((decimal)value, targetTypeInformation);
                case TypeCode.DateTime:
                    return HandleDateTimeValue<T>((DateTime)value, targetTypeInformation);
                case TypeCode.String:
                    return HandleStringValue<T>((string)value, targetTypeInformation);
            }


            return TypeActivator<T>.Instance();
        }

        private static TypeCode GetTypeCode(Type type) {
            var isNullableOf = type.IsNullableType();
            return Type.GetTypeCode(isNullableOf ? Nullable.GetUnderlyingType(type) : type);
        }

        private static T HandleStringValue<T>(string value, TypeInformation targetTypeInformation) {
            if (value == null || value.IsNullOrWhiteSpace()) {
                return TypeActivator<T>.Instance();
            }
            if (targetTypeInformation.IsEnum || (targetTypeInformation.IsNullableOf && targetTypeInformation.BaseType.IsEnum)) {
                return HandleEnum<T>(value);
            }


            switch (targetTypeInformation.TypeCode) {
                case TypeCode.Empty:
                    return TypeActivator<T>.Instance();
                case TypeCode.Object:
                    if ((targetTypeInformation.IsNullableOf ? targetTypeInformation.BaseType : targetTypeInformation.Type) == typeof(Guid)) {
                        Guid tempGuid;
                        var guidParseResult = Guid.TryParse(value, out tempGuid);
                        if (targetTypeInformation.IsNullableOf) {
                            if (guidParseResult) {
                                return BoxingSafeConverter<Guid?, T>.Instance(tempGuid);
                            }

                            return BoxingSafeConverter<Guid?, T>.Instance(null);
                        }
                        if (guidParseResult) {
                            return BoxingSafeConverter<Guid, T>.Instance(tempGuid);
                        }

                        return BoxingSafeConverter<Guid, T>.Instance(Guid.Empty);
                    }

                    break;
                case TypeCode.DBNull:
                    return BoxingSafeConverter<DBNull, T>.Instance(DBNull.Value);
                case TypeCode.Boolean:
                    bool boolValue;
                    if (bool.TryParse(value, out boolValue)) {
                        if (targetTypeInformation.IsNullableOf) {
                            return BoxingSafeConverter<bool?, T>.Instance(boolValue);
                        }

                        return BoxingSafeConverter<bool, T>.Instance(boolValue);
                    }

                    int boolIntValue;
                    if (int.TryParse(value, out boolIntValue)) {
                        if (targetTypeInformation.IsNullableOf) {
                            return BoxingSafeConverter<bool?, T>.Instance(boolIntValue != 0);
                        }

                        return BoxingSafeConverter<bool, T>.Instance(boolIntValue != 0);
                    }

                    break;
                case TypeCode.Char:
                    char charValue;
                    if (char.TryParse(value, out charValue)) {
                        if (targetTypeInformation.IsNullableOf) {
                            return BoxingSafeConverter<char?, T>.Instance(charValue);
                        }

                        return BoxingSafeConverter<char, T>.Instance(charValue);
                    }

                    break;
                case TypeCode.SByte:
                    var sbyteParseResult = ParseNumericStringToRoundedValue(value, Convert.ToSByte, sbyte.TryParse, sbyte.MinValue, sbyte.MaxValue);
                    if (!sbyteParseResult.Key) {
                        break;
                    }

                    var sbyteValue = sbyteParseResult.Value;
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<sbyte?, T>.Instance(sbyteValue);
                    }

                    return BoxingSafeConverter<sbyte, T>.Instance(sbyteValue);
                case TypeCode.Byte:
                    var byteParseResult = ParseNumericStringToRoundedValue(value, Convert.ToByte, byte.TryParse, byte.MinValue, byte.MaxValue);
                    if (!byteParseResult.Key) {
                        break;
                    }

                    var byteValue = byteParseResult.Value;
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<byte?, T>.Instance(byteValue);
                    }

                    return BoxingSafeConverter<byte, T>.Instance(byteValue);
                case TypeCode.Int16:
                    var shortParseResult = ParseNumericStringToRoundedValue(value, Convert.ToInt16, short.TryParse, short.MinValue, short.MaxValue);
                    if (!shortParseResult.Key) {
                        break;
                    }

                    var shortValue = shortParseResult.Value;
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<short?, T>.Instance(shortValue);
                    }

                    return BoxingSafeConverter<short, T>.Instance(shortValue);
                case TypeCode.UInt16:
                    var ushortParseResult = ParseNumericStringToRoundedValue(value, Convert.ToUInt16, ushort.TryParse, ushort.MinValue, ushort.MaxValue);
                    if (!ushortParseResult.Key) {
                        break;
                    }

                    var ushortValue = ushortParseResult.Value;
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<ushort?, T>.Instance(ushortValue);
                    }

                    return BoxingSafeConverter<ushort, T>.Instance(ushortValue);
                case TypeCode.Int32:
                    var intParseResult = ParseNumericStringToRoundedValue(value, Convert.ToInt32, int.TryParse, int.MinValue, int.MaxValue);
                    if (!intParseResult.Key) {
                        break;
                    }

                    var intValue = intParseResult.Value;
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(intValue);
                    }

                    return BoxingSafeConverter<int, T>.Instance(intValue);
                case TypeCode.UInt32:
                    var uintParseResult = ParseNumericStringToRoundedValue(value, Convert.ToUInt32, uint.TryParse, uint.MinValue, uint.MaxValue);
                    if (!uintParseResult.Key) {
                        break;
                    }

                    var uintValue = uintParseResult.Value;
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<uint?, T>.Instance(uintValue);
                    }

                    return BoxingSafeConverter<uint, T>.Instance(uintValue);
                case TypeCode.Int64:
                    var longParseResult = ParseNumericStringToRoundedValue(value, Convert.ToInt64, long.TryParse, long.MinValue, long.MaxValue);
                    if (!longParseResult.Key) {
                        break;
                    }

                    var longValue = longParseResult.Value;
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<long?, T>.Instance(longValue);
                    }

                    return BoxingSafeConverter<long, T>.Instance(longValue);
                case TypeCode.UInt64:
                    var ulongParseResult = ParseNumericStringToRoundedValue(value, Convert.ToUInt64, ulong.TryParse, ulong.MinValue, ulong.MaxValue);
                    if (!ulongParseResult.Key) {
                        break;
                    }

                    var ulongValue = ulongParseResult.Value;
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<ulong?, T>.Instance(ulongValue);
                    }

                    return BoxingSafeConverter<ulong, T>.Instance(ulongValue);
                case TypeCode.Single:
                    var floatValue = default(float);
                    var floatRes = false;
                    if (value.Contains(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator) &&
                        !value.Contains(CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator)) {
                        floatRes = float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture,
                                                    out floatValue);
                    }

                    if (!floatRes) {
                        floatRes = float.TryParse(value, out floatValue);
                    }

                    if (!floatRes) {
                        break;
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<float?, T>.Instance(floatValue);
                    }

                    return BoxingSafeConverter<float, T>.Instance(floatValue);
                case TypeCode.Double:
                    var doubleValue = default(double);
                    var doubleRes = false;
                    if (value.Contains(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator) &&
                        !value.Contains(CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator)) {
                        doubleRes = double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture,
                                                    out doubleValue);
                    }

                    if (!doubleRes) {
                        doubleRes = double.TryParse(value, out doubleValue);
                    }

                    if (!doubleRes) {
                        break;
                    }
                    
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<double?, T>.Instance(doubleValue);
                    }

                    return BoxingSafeConverter<double, T>.Instance(doubleValue);
                case TypeCode.Decimal:
                    var decimalParseResult = ParseStringToDecimalValue(value);
                    if (!decimalParseResult.Key) {
                        break;
                    }

                    var decimalValue = decimalParseResult.Value;
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<decimal?, T>.Instance(decimalValue);
                    }

                    return BoxingSafeConverter<decimal, T>.Instance(decimalValue);
                case TypeCode.DateTime:
                    return BoxingSafeConverter<DateTime, T>.Instance(DateTimeHelpers.Parse(value));
                case TypeCode.String:
                    return BoxingSafeConverter<string, T>.Instance(value);
            }

            return TypeActivator<T>.Instance();
        }
        
        private static KeyValuePair<bool, T> ParseNumericStringToRoundedValue<T>(string value, Func<decimal, T> conversionFunc, TryParseStringFunc<T> tryParseFunc,
                                                             decimal minValue, decimal maxValue) {
            var convertedValue = default(T);
            var didSet = false;

            if (value.Contains(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator) ||
                value.Contains(CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator)) {
                var decimalResult = ParseStringToDecimalValue(value);
                if (decimalResult.Key) {
                    var decimalValue = decimalResult.Value.RoundTo(0);
                    if (decimalValue < minValue || decimalValue > maxValue) {
                        return KeyValuePair.Create(false, convertedValue);
                    }

                    convertedValue = conversionFunc(decimalValue);
                    didSet = true;
                }
            }

            if (!didSet) {
                didSet = tryParseFunc(value, out convertedValue);
            }

            return KeyValuePair.Create(didSet, convertedValue);
        }

        private static KeyValuePair<bool, decimal> ParseStringToDecimalValue(string value) {
            var decimalValue = default(decimal);
            var decimalRes = false;
            if (value.Contains(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator) &&
                !value.Contains(CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator)) {
                decimalRes = decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture,
                                              out decimalValue);
            }

            if (!decimalRes) {
                decimalRes = decimal.TryParse(value, out decimalValue);
            }

            return KeyValuePair.Create(decimalRes, decimalValue);
        }

        private static T HandleDateTimeValue<T>(DateTime value, TypeInformation targetTypeInformation) {
            if (targetTypeInformation.IsEnum) {
                return HandleEnum<T, DateTime>(value, targetTypeInformation);
            }

            if (targetTypeInformation.IsNullableOf && targetTypeInformation.BaseType.IsEnum) {
                return HandleEnum<T, DateTime>(value, targetTypeInformation.BaseType.GetTypeInformation(), targetTypeInformation);
            }

            switch (targetTypeInformation.TypeCode) {
                case TypeCode.Empty:
                case TypeCode.Object:
                    return TypeActivator<T>.Instance();
                case TypeCode.String:
                    return BoxingSafeConverter<string, T>.Instance(value.ToString());
                case TypeCode.DBNull:
                    return BoxingSafeConverter<DBNull, T>.Instance(DBNull.Value);
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return default(T);
                case TypeCode.DateTime:
                    return BoxingSafeConverter<DateTime, T>.Instance(value);
            }

            return TypeActivator<T>.Instance();
        }

        private static T HandleTimeSpanValue<T>(TimeSpan value, TypeInformation targetTypeInformation) {
            if (targetTypeInformation.IsEnum) {
                return HandleEnum<T, TimeSpan>(value, targetTypeInformation);
            }

            if (targetTypeInformation.IsNullableOf && targetTypeInformation.BaseType.IsEnum) {
                return HandleEnum<T, TimeSpan>(value, targetTypeInformation.BaseType.GetTypeInformation(), targetTypeInformation);
            }

            switch (targetTypeInformation.TypeCode) {
                case TypeCode.Empty:
                case TypeCode.Object:
                    return BoxingSafeConverter<TimeSpan, T>.Instance(value);
                case TypeCode.String:
                    return BoxingSafeConverter<string, T>.Instance(value.ToString());
                case TypeCode.DBNull:
                    return BoxingSafeConverter<DBNull, T>.Instance(DBNull.Value);
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.DateTime:
                    return default(T);
            }

            return TypeActivator<T>.Instance();
        }

        private static T HandleDecimalValue<T>(decimal value, TypeInformation targetTypeInformation) {
            if (targetTypeInformation.IsEnum) {
                return HandleEnum<T, decimal>(value, targetTypeInformation);
            }

            if (targetTypeInformation.IsNullableOf && targetTypeInformation.BaseType.IsEnum) {
                return HandleEnum<T, decimal>(value, targetTypeInformation.BaseType.GetTypeInformation(), targetTypeInformation);
            }

            switch (targetTypeInformation.TypeCode) {
                case TypeCode.Empty:
                case TypeCode.Object:
                    return TypeActivator<T>.Instance();
                case TypeCode.String:
                    return BoxingSafeConverter<string, T>.Instance(value.ToString());
                case TypeCode.DBNull:
                    return BoxingSafeConverter<DBNull, T>.Instance(DBNull.Value);
                case TypeCode.Boolean:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<bool?, T>.Instance(value != 0);
                    }

                    return BoxingSafeConverter<bool, T>.Instance(value != 0);
                case TypeCode.Char:
                    return default(T);
                case TypeCode.SByte:
                    if (value < ConverterConstants.Decimal.SByteMinValue || value > ConverterConstants.Decimal.SByteMaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(Convert.ToInt32(value));
                    }

                    return BoxingSafeConverter<int, T>.Instance(Convert.ToInt32(value));
                case TypeCode.Byte:
                    if (value < ConverterConstants.Decimal.ByteMinValue || value > ConverterConstants.Decimal.ByteMaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(Convert.ToInt32(value));
                    }

                    return BoxingSafeConverter<int, T>.Instance(Convert.ToInt32(value));
                case TypeCode.Int16:
                    if (value < ConverterConstants.Decimal.Int16MinValue || value > ConverterConstants.Decimal.Int16MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(Convert.ToInt32(value));
                    }

                    return BoxingSafeConverter<int, T>.Instance(Convert.ToInt32(value));
                case TypeCode.UInt16:
                    if (value < ConverterConstants.Decimal.UInt16MinValue || value > ConverterConstants.Decimal.UInt16MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(Convert.ToInt32(value));
                    }

                    return BoxingSafeConverter<int, T>.Instance(Convert.ToInt32(value));
                case TypeCode.Int32:
                    if (value < ConverterConstants.Decimal.Int32MinValue || value > ConverterConstants.Decimal.Int32MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(Convert.ToInt32(value));
                    }

                    return BoxingSafeConverter<int, T>.Instance(Convert.ToInt32(value));
                case TypeCode.UInt32:
                    if (value < ConverterConstants.Decimal.UInt32MinValue || value > ConverterConstants.Decimal.UInt32MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<uint?, T>.Instance(Convert.ToUInt32(value));
                    }

                    return BoxingSafeConverter<uint, T>.Instance(Convert.ToUInt32(value));
                case TypeCode.Int64:
                    if (value < ConverterConstants.Decimal.Int64MinValue || value > ConverterConstants.Decimal.Int64MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<long?, T>.Instance(Convert.ToInt64(value));
                    }

                    return BoxingSafeConverter<long, T>.Instance(Convert.ToInt64(value));
                case TypeCode.UInt64:
                    if (value < ConverterConstants.Decimal.UInt64MinValue || value > ConverterConstants.Decimal.UInt64MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<ulong?, T>.Instance(Convert.ToUInt64(value));
                    }

                    return BoxingSafeConverter<ulong, T>.Instance(Convert.ToUInt64(value));
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<decimal?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<decimal, T>.Instance(value);
                case TypeCode.DateTime:
                    return BoxingSafeConverter<DateTime, T>.Instance(DateTimeHelpers.BaseDate);
            }

            return TypeActivator<T>.Instance();
        }

        private static T HandleDoubleValue<T>(double value, TypeInformation targetTypeInformation) {
            if (targetTypeInformation.IsEnum) {
                return HandleEnum<T, double>(value, targetTypeInformation);
            }

            if (targetTypeInformation.IsNullableOf && targetTypeInformation.BaseType.IsEnum) {
                return HandleEnum<T, double>(value, targetTypeInformation.BaseType.GetTypeInformation(), targetTypeInformation);
            }

            switch (targetTypeInformation.TypeCode) {
                case TypeCode.Empty:
                case TypeCode.Object:
                    return TypeActivator<T>.Instance();
                case TypeCode.String:
                    return BoxingSafeConverter<string, T>.Instance(value.ToString());
                case TypeCode.DBNull:
                    return BoxingSafeConverter<DBNull, T>.Instance(DBNull.Value);
                case TypeCode.Boolean:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<bool?, T>.Instance(value != 0);
                    }

                    return BoxingSafeConverter<bool, T>.Instance(value != 0);
                case TypeCode.Char:
                    return default(T);
                case TypeCode.SByte:
                    if (value < sbyte.MinValue || value > sbyte.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(Convert.ToInt32(value));
                    }

                    return BoxingSafeConverter<int, T>.Instance(Convert.ToInt32(value));
                case TypeCode.Byte:
                    if (value < 0 || value > byte.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(Convert.ToInt32(value));
                    }

                    return BoxingSafeConverter<int, T>.Instance(Convert.ToInt32(value));
                case TypeCode.Int16:
                    if (value < short.MinValue || value > short.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(Convert.ToInt32(value));
                    }

                    return BoxingSafeConverter<int, T>.Instance(Convert.ToInt32(value));
                case TypeCode.UInt16:
                    if (value < 0 || value > ushort.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(Convert.ToInt32(value));
                    }

                    return BoxingSafeConverter<int, T>.Instance(Convert.ToInt32(value));
                case TypeCode.Int32:
                    if (value < int.MinValue || value > int.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(Convert.ToInt32(value));
                    }

                    return BoxingSafeConverter<int, T>.Instance(Convert.ToInt32(value));
                case TypeCode.UInt32:
                    if (value < 0 || value > uint.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<uint?, T>.Instance(Convert.ToUInt32(value));
                    }

                    return BoxingSafeConverter<uint, T>.Instance(Convert.ToUInt32(value));
                case TypeCode.Int64:
                    if (value < long.MinValue || value > long.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<long?, T>.Instance(Convert.ToInt64(value));
                    }

                    return BoxingSafeConverter<long, T>.Instance(Convert.ToInt64(value));
                case TypeCode.UInt64:
                    if (value < 0 || value > ulong.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<ulong?, T>.Instance(Convert.ToUInt64(value));
                    }

                    return BoxingSafeConverter<ulong, T>.Instance(Convert.ToUInt64(value));
                case TypeCode.Single:
                    if (value < float.MinValue || value > float.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<double?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<double, T>.Instance(value);
                case TypeCode.Double:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<double?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<double, T>.Instance(value);
                case TypeCode.Decimal:
                    if (value < (double)decimal.MinValue || value > (double)decimal.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<double?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<double, T>.Instance(value);
                case TypeCode.DateTime:
                    return BoxingSafeConverter<DateTime, T>.Instance(DateTimeHelpers.BaseDate);
            }

            return TypeActivator<T>.Instance();
        }

        private static T HandleSingleValue<T>(float value, TypeInformation targetTypeInformation) {
            if (targetTypeInformation.IsEnum) {
                return HandleEnum<T, float>(value, targetTypeInformation);
            }

            if (targetTypeInformation.IsNullableOf && targetTypeInformation.BaseType.IsEnum) {
                return HandleEnum<T, float>(value, targetTypeInformation.BaseType.GetTypeInformation(), targetTypeInformation);
            }

            switch (targetTypeInformation.TypeCode) {
                case TypeCode.Empty:
                case TypeCode.Object:
                    return TypeActivator<T>.Instance();
                case TypeCode.String:
                    return BoxingSafeConverter<string, T>.Instance(value.ToString());
                case TypeCode.DBNull:
                    return BoxingSafeConverter<DBNull, T>.Instance(DBNull.Value);
                case TypeCode.Boolean:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<bool?, T>.Instance(value != 0);
                    }

                    return BoxingSafeConverter<bool, T>.Instance(value != 0);
                case TypeCode.Char:
                    return default(T);
                case TypeCode.SByte:
                    if (value < sbyte.MinValue || value > sbyte.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(Convert.ToInt32(value));
                    }

                    return BoxingSafeConverter<int, T>.Instance(Convert.ToInt32(value));
                case TypeCode.Byte:
                    if (value < 0 || value > byte.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(Convert.ToInt32(value));
                    }

                    return BoxingSafeConverter<int, T>.Instance(Convert.ToInt32(value));
                case TypeCode.Int16:
                    if (value < short.MinValue || value > short.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(Convert.ToInt32(value));
                    }

                    return BoxingSafeConverter<int, T>.Instance(Convert.ToInt32(value));
                case TypeCode.UInt16:
                    if (value < 0 || value > uint.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(Convert.ToInt32(value));
                    }

                    return BoxingSafeConverter<int, T>.Instance(Convert.ToInt32(value));
                case TypeCode.Int32:
                    if (value < int.MinValue || value > int.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(Convert.ToInt32(value));
                    }

                    return BoxingSafeConverter<int, T>.Instance(Convert.ToInt32(value));
                case TypeCode.UInt32:
                    if (value < 0 || value > uint.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<uint?, T>.Instance(Convert.ToUInt32(value));
                    }

                    return BoxingSafeConverter<uint, T>.Instance(Convert.ToUInt32(value));
                case TypeCode.Int64:
                    if (value < long.MinValue || value > long.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<long?, T>.Instance(Convert.ToInt64(value));
                    }

                    return BoxingSafeConverter<long, T>.Instance(Convert.ToInt64(value));
                case TypeCode.UInt64:
                    if (value < 0 || value > ulong.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<ulong?, T>.Instance(Convert.ToUInt64(value));
                    }

                    return BoxingSafeConverter<ulong, T>.Instance(Convert.ToUInt64(value));
                case TypeCode.Single:
                case TypeCode.Double:
                    return BoxingSafeConverter<float, T>.Instance(value);
                case TypeCode.Decimal:
                    if (value < (float)decimal.MinValue || value > (float)decimal.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<float?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<float, T>.Instance(value);
                case TypeCode.DateTime:
                    return BoxingSafeConverter<DateTime, T>.Instance(DateTimeHelpers.BaseDate);
            }

            return TypeActivator<T>.Instance();
        }

        private static T HandleInt64Value<T>(long value, TypeInformation targetTypeInformation) {
            if (targetTypeInformation.IsEnum) {
                return HandleEnum<T, long>(value, targetTypeInformation);
            }

            if (targetTypeInformation.IsNullableOf && targetTypeInformation.BaseType.IsEnum) {
                return HandleEnum<T, long>(value, targetTypeInformation.BaseType.GetTypeInformation(), targetTypeInformation);
            }

            switch (targetTypeInformation.TypeCode) {
                case TypeCode.Empty:
                case TypeCode.Object:
                    return TypeActivator<T>.Instance();
                case TypeCode.String:
                    return BoxingSafeConverter<string, T>.Instance(value.ToString());
                case TypeCode.DBNull:
                    return BoxingSafeConverter<DBNull, T>.Instance(DBNull.Value);
                case TypeCode.Boolean:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<bool?, T>.Instance(value != 0);
                    }

                    return BoxingSafeConverter<bool, T>.Instance(value != 0);
                case TypeCode.Char:
                    if (value < char.MinValue || value > char.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<decimal?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<decimal, T>.Instance(value);
                case TypeCode.SByte:
                    if (value < sbyte.MinValue || value > sbyte.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<decimal?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<decimal, T>.Instance(value);
                case TypeCode.Byte:
                    if (value < 0 || value > byte.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<decimal?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<decimal, T>.Instance(value);
                case TypeCode.Int16:
                    if (value < short.MinValue || value > short.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<decimal?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<decimal, T>.Instance(value);
                case TypeCode.UInt16:
                    if (value < 0 || value > ushort.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<decimal?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<decimal, T>.Instance(value);
                case TypeCode.Int32:
                    if (value < int.MinValue || value > int.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<decimal?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<decimal, T>.Instance(value);
                case TypeCode.UInt32:
                    if (value < 0 || value > uint.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<decimal?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<decimal, T>.Instance(value);
                case TypeCode.UInt64:
                    if (value < 0) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<decimal?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<decimal, T>.Instance(value);
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<decimal?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<decimal, T>.Instance(value);
                case TypeCode.DateTime:
                    return BoxingSafeConverter<DateTime, T>.Instance(DateTimeHelpers.BaseDate);
            }

            return TypeActivator<T>.Instance();
        }

        private static T HandleUInt64Value<T>(ulong value, TypeInformation targetTypeInformation) {
            if (targetTypeInformation.IsEnum) {
                return HandleEnum<T, ulong>(value, targetTypeInformation);
            }

            if (targetTypeInformation.IsNullableOf && targetTypeInformation.BaseType.IsEnum) {
                return HandleEnum<T, ulong>(value, targetTypeInformation.BaseType.GetTypeInformation(), targetTypeInformation);
            }

            switch (targetTypeInformation.TypeCode) {
                case TypeCode.Empty:
                case TypeCode.Object:
                    return TypeActivator<T>.Instance();
                case TypeCode.String:
                    return BoxingSafeConverter<string, T>.Instance(value.ToString());
                case TypeCode.DBNull:
                    return BoxingSafeConverter<DBNull, T>.Instance(DBNull.Value);
                case TypeCode.Boolean:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<bool?, T>.Instance(value != 0);
                    }

                    return BoxingSafeConverter<bool, T>.Instance(value != 0);
                case TypeCode.Char:
                    if (value < char.MinValue || value > char.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<ulong?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<ulong, T>.Instance(value);
                case TypeCode.SByte:
                    if (value > (ulong)sbyte.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<ulong?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<ulong, T>.Instance(value);
                case TypeCode.Byte:
                    if (value > byte.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<ulong?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<ulong, T>.Instance(value);
                case TypeCode.Int16:
                    if (value > (ulong)short.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<ulong?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<ulong, T>.Instance(value);
                case TypeCode.UInt16:
                    if (value > ushort.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<ulong?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<ulong, T>.Instance(value);
                case TypeCode.Int32:
                    if (value > int.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<ulong?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<ulong, T>.Instance(value);
                case TypeCode.UInt32:
                    if (value > uint.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<ulong?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<ulong, T>.Instance(value);
                case TypeCode.Int64:
                    if (value > long.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<ulong?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<ulong, T>.Instance(value);
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<ulong?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<ulong, T>.Instance(value);
                case TypeCode.DateTime:
                    return BoxingSafeConverter<DateTime, T>.Instance(DateTimeHelpers.BaseDate);
            }

            return TypeActivator<T>.Instance();
        }

        private static T HandleUInt32Value<T>(uint value, TypeInformation targetTypeInformation) {
            if (targetTypeInformation.IsEnum) {
                return HandleEnum<T, uint>(value, targetTypeInformation);
            }

            if (targetTypeInformation.IsNullableOf && targetTypeInformation.BaseType.IsEnum) {
                return HandleEnum<T, uint>(value, targetTypeInformation.BaseType.GetTypeInformation(), targetTypeInformation);
            }

            switch (targetTypeInformation.TypeCode) {
                case TypeCode.Empty:
                case TypeCode.Object:
                    return TypeActivator<T>.Instance();
                case TypeCode.String:
                    return BoxingSafeConverter<string, T>.Instance(value.ToString());
                case TypeCode.DBNull:
                    return BoxingSafeConverter<DBNull, T>.Instance(DBNull.Value);
                case TypeCode.Boolean:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<bool?, T>.Instance(value != 0);
                    }

                    return BoxingSafeConverter<bool, T>.Instance(value != 0);
                case TypeCode.Char:
                    if (value < char.MinValue || value > char.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<uint?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<uint, T>.Instance(value);
                case TypeCode.SByte:
                    if (value > sbyte.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<uint?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<uint, T>.Instance(value);
                case TypeCode.Byte:
                    if (value > byte.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<uint?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<uint, T>.Instance(value);
                case TypeCode.Int16:
                    if (value > short.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<uint?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<uint, T>.Instance(value);
                case TypeCode.UInt16:
                    if (value > ushort.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<uint?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<uint, T>.Instance(value);
                case TypeCode.Int32:
                    if (value > int.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<uint?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<uint, T>.Instance(value);
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<uint?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<uint, T>.Instance(value);
                case TypeCode.DateTime:
                    return BoxingSafeConverter<DateTime, T>.Instance(DateTimeHelpers.BaseDate);
            }

            return TypeActivator<T>.Instance();
        }

        private static T HandleInt32Value<T>(int value, TypeInformation targetTypeInformation) {
            if (targetTypeInformation.IsEnum) {
                return HandleEnum<T, int>(value, targetTypeInformation);
            }

            if (targetTypeInformation.IsNullableOf && targetTypeInformation.BaseType.IsEnum) {
                return HandleEnum<T, int>(value, targetTypeInformation.BaseType.GetTypeInformation(), targetTypeInformation);
            }

            switch (targetTypeInformation.TypeCode) {
                case TypeCode.Empty:
                case TypeCode.Object:
                    if (targetTypeInformation.Type == typeof(Guid)) {
                        return default(T);
                    }

                    return TypeActivator<T>.Instance();
                case TypeCode.String:
                    return BoxingSafeConverter<string, T>.Instance(value.ToString());
                case TypeCode.DBNull:
                    return BoxingSafeConverter<DBNull, T>.Instance(DBNull.Value);
                case TypeCode.Boolean:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<bool?, T>.Instance(value != 0);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<bool, T>.Instance(value != 0);
                case TypeCode.Char:
                    if (value < char.MinValue || value > char.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<int, T>.Instance(value);
                case TypeCode.SByte:
                    if (value < sbyte.MinValue || value > sbyte.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<int, T>.Instance(value);
                case TypeCode.Byte:
                    if (value < 0 || value > byte.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<int, T>.Instance(value);
                case TypeCode.Int16:
                    if (value < short.MinValue || value > short.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<int, T>.Instance(value);
                case TypeCode.UInt16:
                    if (value < 0 || value > ushort.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<int, T>.Instance(value);
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    if (value < 0) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<int, T>.Instance(value);
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<int?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<int, T>.Instance(value);
                case TypeCode.DateTime:
                    return BoxingSafeConverter<DateTime, T>.Instance(DateTimeHelpers.BaseDate);
            }

            return TypeActivator<T>.Instance();
        }

        private static T HandleUInt16Value<T>(ushort value, TypeInformation targetTypeInformation) {
            if (targetTypeInformation.IsEnum) {
                return HandleEnum<T, ushort>(value, targetTypeInformation);
            }

            if (targetTypeInformation.IsNullableOf && targetTypeInformation.BaseType.IsEnum) {
                return HandleEnum<T, ushort>(value, targetTypeInformation.BaseType.GetTypeInformation(), targetTypeInformation);
            }

            switch (targetTypeInformation.TypeCode) {
                case TypeCode.Empty:
                case TypeCode.Object:
                    return TypeActivator<T>.Instance();
                case TypeCode.String:
                    return BoxingSafeConverter<string, T>.Instance(value.ToString());
                case TypeCode.DBNull:
                    return BoxingSafeConverter<DBNull, T>.Instance(DBNull.Value);
                case TypeCode.Boolean:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<bool?, T>.Instance(value != 0);
                    }

                    return BoxingSafeConverter<bool, T>.Instance(value != 0);
                case TypeCode.Char:
                    if (value < char.MinValue || value > char.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<ushort?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<ushort, T>.Instance(value);
                case TypeCode.SByte:
                    if (value > sbyte.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<ushort?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<ushort, T>.Instance(value);
                case TypeCode.Byte:
                    if (value > byte.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<ushort?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<ushort, T>.Instance(value);
                case TypeCode.Int16:
                    if (value > short.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<ushort?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<ushort, T>.Instance(value);
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<ushort?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<ushort, T>.Instance(value);
                case TypeCode.DateTime:
                    return BoxingSafeConverter<DateTime, T>.Instance(DateTimeHelpers.BaseDate);
            }

            return TypeActivator<T>.Instance();
        }

        private static T HandleInt16Value<T>(short value, TypeInformation targetTypeInformation) {
            if (targetTypeInformation.IsEnum) {
                return HandleEnum<T, short>(value, targetTypeInformation);
            }

            if (targetTypeInformation.IsNullableOf && targetTypeInformation.BaseType.IsEnum) {
                return HandleEnum<T, short>(value, targetTypeInformation.BaseType.GetTypeInformation(), targetTypeInformation);
            }
            switch (targetTypeInformation.TypeCode) {
                case TypeCode.Empty:
                case TypeCode.Object:
                    return TypeActivator<T>.Instance();
                case TypeCode.String:
                    return BoxingSafeConverter<string, T>.Instance(value.ToString());
                case TypeCode.DBNull:
                    return BoxingSafeConverter<DBNull, T>.Instance(DBNull.Value);
                case TypeCode.Boolean:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<bool?, T>.Instance(value != 0);
                    }

                    return BoxingSafeConverter<bool, T>.Instance(value != 0);
                case TypeCode.SByte:
                    if (value < sbyte.MinValue || value > sbyte.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<short?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<short, T>.Instance(value);
                case TypeCode.Byte:
                    if (value < 0 || value > byte.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<short?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<short, T>.Instance(value);
                case TypeCode.Char:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    if (value < 0) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<short?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<short, T>.Instance(value);
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<short?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<short, T>.Instance(value);
                case TypeCode.DateTime:
                    return BoxingSafeConverter<DateTime, T>.Instance(DateTimeHelpers.BaseDate);
            }

            return TypeActivator<T>.Instance();
        }

        private static T HandleByteValue<T>(byte value, TypeInformation targetTypeInformation) {
            if (targetTypeInformation.IsEnum) {
                return HandleEnum<T, byte>(value, targetTypeInformation);
            }

            if (targetTypeInformation.IsNullableOf && targetTypeInformation.BaseType.IsEnum) {
                return HandleEnum<T, byte>(value, targetTypeInformation.BaseType.GetTypeInformation(), targetTypeInformation);
            }

            switch (targetTypeInformation.TypeCode) {
                case TypeCode.Empty:
                case TypeCode.Object:
                    return TypeActivator<T>.Instance();
                case TypeCode.String:
                    return BoxingSafeConverter<string, T>.Instance(value.ToString());
                case TypeCode.DBNull:
                    return BoxingSafeConverter<DBNull, T>.Instance(DBNull.Value);
                case TypeCode.Boolean:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<bool?, T>.Instance(value != 0);
                    }

                    return BoxingSafeConverter<bool, T>.Instance(value != 0);
                case TypeCode.SByte:
                    if (value > sbyte.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<byte?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<byte, T>.Instance(value);
                case TypeCode.Char:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<byte?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<byte, T>.Instance(value);
                case TypeCode.DateTime:
                    return BoxingSafeConverter<DateTime, T>.Instance(DateTimeHelpers.BaseDate);
            }

            return TypeActivator<T>.Instance();
        }

        private static T HandleSByteValue<T>(sbyte value, TypeInformation targetTypeInformation) {
            if (targetTypeInformation.IsEnum) {
                return HandleEnum<T, sbyte>(value, targetTypeInformation);
            }

            if (targetTypeInformation.IsNullableOf && targetTypeInformation.BaseType.IsEnum) {
                return HandleEnum<T, sbyte>(value, targetTypeInformation.BaseType.GetTypeInformation(), targetTypeInformation);
            }

            switch (targetTypeInformation.TypeCode) {
                case TypeCode.Empty:
                case TypeCode.Object:
                    return TypeActivator<T>.Instance();
                case TypeCode.String:
                    return BoxingSafeConverter<string, T>.Instance(value.ToString());
                case TypeCode.DBNull:
                    return BoxingSafeConverter<DBNull, T>.Instance(DBNull.Value);
                case TypeCode.Boolean:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<bool?, T>.Instance(value != 0);
                    }

                    return BoxingSafeConverter<bool, T>.Instance(value != 0);
                case TypeCode.Char:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    if (value < 0) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<sbyte?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<sbyte, T>.Instance(value);
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<sbyte?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<sbyte, T>.Instance(value);
                case TypeCode.DateTime:
                    return BoxingSafeConverter<DateTime, T>.Instance(DateTimeHelpers.BaseDate);
            }

            return TypeActivator<T>.Instance();
        }

        private static T HandleCharValue<T>(char value, TypeInformation targetTypeInformation) {
            if (targetTypeInformation.IsEnum) {
                return HandleEnum<T, char>(value, targetTypeInformation);
            }

            if (targetTypeInformation.IsNullableOf && targetTypeInformation.BaseType.IsEnum) {
                return HandleEnum<T, char>(value, targetTypeInformation.BaseType.GetTypeInformation(), targetTypeInformation);
            }

            switch (targetTypeInformation.TypeCode) {
                case TypeCode.Empty:
                case TypeCode.Object:
                    return TypeActivator<T>.Instance();
                case TypeCode.String:
                    return BoxingSafeConverter<string, T>.Instance(value.ToString());
                case TypeCode.DBNull:
                    return BoxingSafeConverter<DBNull, T>.Instance(DBNull.Value);
                case TypeCode.Boolean:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<bool?, T>.Instance(value != 0);
                    }

                    return BoxingSafeConverter<bool, T>.Instance(value != 0);
                case TypeCode.Char:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<char?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<char, T>.Instance(value);
                case TypeCode.SByte:
                    if (value > sbyte.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<char?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<char, T>.Instance(value);
                case TypeCode.Byte:
                    if (value > byte.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<char?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<char, T>.Instance(value);
                case TypeCode.Int16:
                    if (value > byte.MaxValue) {
                        return default(T);
                    }

                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<char?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<char, T>.Instance(value);
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<char?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<char, T>.Instance(value);
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return default(T);
                case TypeCode.DateTime:
                    return BoxingSafeConverter<DateTime, T>.Instance(DateTimeHelpers.BaseDate);
            }

            return TypeActivator<T>.Instance();
        }

        private static T HandleBooleanValue<T>(bool value, TypeInformation targetTypeInformation) {
            if (targetTypeInformation.IsEnum) {
                return HandleEnum<T, bool>(value, targetTypeInformation);
            }

            if (targetTypeInformation.IsNullableOf && targetTypeInformation.BaseType.IsEnum) {
                return HandleEnum<T, bool>(value, targetTypeInformation.BaseType.GetTypeInformation(), targetTypeInformation);
            }

            switch (targetTypeInformation.TypeCode) {
                case TypeCode.Empty:
                case TypeCode.Object:
                    return TypeActivator<T>.Instance();
                case TypeCode.String:
                    return BoxingSafeConverter<string, T>.Instance(value.ToString());
                case TypeCode.DBNull:
                    return BoxingSafeConverter<DBNull, T>.Instance(DBNull.Value);
                case TypeCode.Boolean:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<bool?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<bool, T>.Instance(value);
                case TypeCode.Char:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<char?, T>.Instance((char)(value ? 1 : 0));
                    }

                    return BoxingSafeConverter<char, T>.Instance((char)(value ? 1 : 0));
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<bool?, T>.Instance(value);
                    }

                    return BoxingSafeConverter<bool, T>.Instance(value);
                case TypeCode.Decimal:
                    if (targetTypeInformation.IsNullableOf) {
                        return BoxingSafeConverter<decimal?, T>.Instance(value ? 1m : 0m);
                    }

                    return BoxingSafeConverter<decimal, T>.Instance(value ? 1m : 0m);
                case TypeCode.DateTime:
                    return BoxingSafeConverter<DateTime, T>.Instance(DateTimeHelpers.BaseDate);
            }

            return TypeActivator<T>.Instance();
        }

        private static T HandleEnum<T, TValue>(TValue value, TypeInformation typeInformation, TypeInformation parentTypeInformation = null) where TValue : struct {
            var converter = GetDelegate(typeInformation.BaseType);
            var baseValue = converter(value);
            if (EnumHelper.IsDefined(baseValue, typeInformation.Type)) {
                if (parentTypeInformation != null) {
                    converter = GetDelegate(parentTypeInformation.BaseType);
                    baseValue = converter(baseValue);
                }
                return BoxingSafeConverter<object, T>.Instance(baseValue);
            }

            return default(T);
        }

        private static T HandleObjectToEnum<T>(object value, TypeCode valueTypeCode, TypeInformation typeInformation) {
            if (valueTypeCode == TypeCode.String) {
                return EnumSafeConverter<T>.Instance(value as string);
            }
            var converter = GetDelegate(typeInformation.BaseType);
            var baseValue = converter(value);
            if (EnumHelper.IsDefined(baseValue, typeInformation.Type)) {
                return BoxingSafeConverter<object, T>.Instance(baseValue);
            }

            return default(T);
        }

        private static T HandleEnum<T>(string value) {
            return EnumSafeConverter<T>.Instance(value);
        }
    }
}