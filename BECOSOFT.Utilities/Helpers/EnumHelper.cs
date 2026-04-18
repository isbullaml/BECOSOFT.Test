using BECOSOFT.Utilities.Attributes;
using BECOSOFT.Utilities.Collections;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;

namespace BECOSOFT.Utilities.Helpers {
    /// <summary>
    /// Helper class for an <see cref="Enum"/>
    /// </summary>
    public static class EnumHelper {
        public static string GetDescription(this Enum @enum) {
            return GetDescription<DescriptionAttribute>(@enum);
        }
        /// <summary>
        /// Gets the localized description of an <see cref="Enum"/>
        /// </summary>
        /// <param name="enum">The enum</param>
        /// <returns>The localized description</returns>
        public static string GetLocalizedDescription(this Enum @enum) {
            return GetDescription<LocalizedEnumAttribute>(@enum);
        }

        /// <summary>
        /// Gets the localized description of an <see cref="Enum"/>
        /// </summary>
        /// <param name="enum">The enum</param>
        /// <param name="cultureInfo"></param>
        /// <returns>The localized description</returns>
        public static string GetLocalizedDescription(this Enum @enum, CultureInfo cultureInfo) {
            return GetDescription<LocalizedEnumAttribute>(@enum, cultureInfo);
        }

        /// <summary>
        /// Gets the localized abbreviation of an <see cref="Enum"/>
        /// </summary>
        /// <param name="enum">The enum</param>
        /// <returns>The localized description</returns>
        public static string GetLocalizedAbbreviation(this Enum @enum) {
            return GetDescription<LocalizedAbbreviationAttribute>(@enum);
        }

        /// <summary>
        /// Gets the localized abbreviation of an <see cref="Enum"/>
        /// </summary>
        /// <param name="enum">The enum</param>
        /// <param name="cultureInfo"></param>
        /// <returns>The localized description</returns>
        public static string GetLocalizedAbbreviation(this Enum @enum, CultureInfo cultureInfo) {
            return GetDescription<LocalizedAbbreviationAttribute>(@enum, cultureInfo);
        }

        /// <summary>
        /// Gets the localized plural description of an <see cref="Enum"/>
        /// </summary>
        /// <param name="enum">The enum</param>
        /// <returns>The localized plural description</returns>
        public static string GetLocalizedPluralDescription(this Enum @enum) {
            return GetDescription<LocalizedPluralEnumAttribute>(@enum);
        }

        private static string GetDescription<T>(this Enum @enum) where T : DescriptionAttribute {
            if (@enum == null) {
                return null;
            }
            var description = @enum.ToString();
            var enumType = @enum.GetType();
            T[] attributes = null;
            if (Enum.IsDefined(enumType, @enum)) {
                var fieldInfo = @enum.GetType().GetField(description);
                attributes = (T[]) fieldInfo.GetCustomAttributes(typeof(T), true);
            }
            return attributes?.FirstOrDefault()?.Description ?? description;
        }

        private static string GetDescription<T>(this Enum @enum, CultureInfo cultureInfo) where T : LocalizedAttribute {
            if (@enum == null) {
                return null;
            }
            var description = @enum.ToString();
            var enumType = @enum.GetType();
            T[] attributes = null;
            if (Enum.IsDefined(enumType, @enum)) {
                var fieldInfo = @enum.GetType().GetField(description);
                attributes = (T[]) fieldInfo.GetCustomAttributes(typeof(T), true);
            }
            var attribute = attributes?.FirstOrDefault();
            var key = attribute?.DisplayNameKey;
            var resourceType = attribute?.NameResourceType;
            if (key == null || resourceType == null) {
                return description;
            }

            var resourceManager = new ResourceManager(resourceType);
            return resourceManager.GetString(key, cultureInfo) ?? description;
        }


        /// <summary>
        /// Gets the localized descriptions of an <see cref="Enum"/>
        /// </summary>
        /// <param name="enum">The enum</param>
        /// <param name="languages">The languages</param>
        /// <returns>The localized descriptions</returns>
        public static List<string> GetLocalizedDescriptions(this Enum @enum, IEnumerable<CultureInfo> languages) {
            return GetDescriptions<LocalizedEnumAttribute>(@enum, languages);
        }

        /// <summary>
        /// Gets the localized abbreviations of an <see cref="Enum"/>
        /// </summary>
        /// <param name="enum">The enum</param>
        /// <param name="languages">The languages</param>
        /// <returns>The localized descriptions</returns>
        public static List<string> GetLocalizedAbbreviations(this Enum @enum, IEnumerable<CultureInfo> languages) {
            return GetDescriptions<LocalizedAbbreviationAttribute>(@enum, languages);
        }

        /// <summary>
        /// Gets the localized plural descriptions of an <see cref="Enum"/>
        /// </summary>
        /// <param name="enum">The enum</param>
        /// <param name="languages">The languages</param>
        /// <returns>The localized plural descriptions</returns>
        public static List<string> GetLocalizedPluralDescriptions(this Enum @enum, IEnumerable<CultureInfo> languages) {
            return GetDescriptions<LocalizedPluralEnumAttribute>(@enum, languages);
        }

        /// <summary>
        /// Gets the descriptions
        /// </summary>
        /// <typeparam name="T">The type of enum</typeparam>
        /// <param name="enum">The enum</param>
        /// <param name="languages">The languages</param>
        /// <returns>A list of localized descriptions of the enum</returns>
        public static List<string> GetDescriptions<T>(this Enum @enum, IEnumerable<CultureInfo> languages) where T : LocalizedAttribute {
            if (@enum == null) {
                return new List<string>();
            }
            var description = @enum.ToString();
            var enumType = @enum.GetType();
            T[] attributes = null;
            if (Enum.IsDefined(enumType, @enum)) {
                var fieldInfo = @enum.GetType().GetField(description);
                attributes = (T[]) fieldInfo.GetCustomAttributes(typeof(T), true);
            }

            var attribute = attributes?.FirstOrDefault();
            var key = attribute?.DisplayNameKey;
            var resourceType = attribute?.NameResourceType;
            if (key == null || resourceType == null) {
                return new List<string> { description };
            }

            var resourceManager = new ResourceManager(resourceType);
            return languages.Select(language => resourceManager.GetString(key, language))
                            .Where(value => !value.IsNullOrWhiteSpace())
                            .ToList();
        }

        /// <summary>
        /// Gets the enum as a <see cref="KeyValueList{TKey,TValue}"/>
        /// </summary>
        /// <typeparam name="TEnum">The type of the key</typeparam>
        /// <returns>The <see cref="KeyValueList{TKey,TValue}"/> containing the enum values (as key) and the localized descriptions as value</returns>
        public static KeyValueList<TEnum, string> GetKeyValueList<TEnum>() where TEnum : Enum {
            var type = typeof(TEnum);
            var typeInformation = type.GetTypeInformation();
            if (!typeInformation.IsEnum) {
                throw new InvalidEnumArgumentException();
            }
            var enumValues = Enum.GetValues(type);
            var result = new KeyValueList<TEnum, string>();
            foreach (var enumValue in enumValues) {
                result.Add(Converter.GetValue<TEnum>(enumValue), GetLocalizedDescription((Enum) enumValue));
            }
            return result;
        }

        /// <summary>
        /// Gets the enum as a <see cref="KeyValueList{TKey,TValue}"/>
        /// </summary>
        /// <typeparam name="T">The type of the key</typeparam>
        /// <param name="type">The type of the enum</param>
        /// <returns>The <see cref="KeyValueList{TKey,TValue}"/> containing the enum values (as key) and the localized descriptions as value</returns>
        public static KeyValueList<T, string> GetKeyValueList<T>(Type type) where T : struct {
            var typeInformation = type.GetTypeInformation();
            if (!typeInformation.IsEnum) {
                throw new InvalidEnumArgumentException();
            }
            var enumValues = Enum.GetValues(type);
            var result = new KeyValueList<T, string>();
            foreach (var enumValue in enumValues) {
                result.Add(Converter.GetValue<T>(enumValue), GetLocalizedDescription((Enum) enumValue));
            }
            return result;
        }

        /// <summary>
        /// Gets the values of an enum
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <returns>The list of values</returns>
        public static List<T> GetValues<T>() where T : Enum {
            var type = typeof(T);
            return Enum.GetValues(type).Cast<T>().OrderBy(e => e).ToList();
        }

        public static List<TValue> GetFlagValues<TEnum, TValue>(TEnum options) where TEnum : Enum {
            var type = typeof(TEnum);
            var typeInformation = type.GetTypeInformation();
            if (!typeInformation.IsEnumWithFlag) {
                throw new InvalidEnumArgumentException();
            }
            var values = GetValues<TEnum>();
            var hasFlagMethod = type.GetMethod("HasFlag");
            if (hasFlagMethod == null) {
                throw new NullReferenceException();
            }
            return values.Where(e => !e.Equals(default(TEnum)) && hasFlagMethod.Invoke(options, new object[] { e }).To<bool>())
                         .Select(e => e.To<TValue>()).ToList();
        }

        public static List<TEnum> GetFlagValues<TEnum>(TEnum options) where TEnum : Enum {
            var type = typeof(TEnum);
            var typeInformation = type.GetTypeInformation();
            if (!typeInformation.IsEnumWithFlag) {
                throw new InvalidEnumArgumentException();
            }
            var values = GetValues<TEnum>();
            var hasFlagMethod = type.GetMethod("HasFlag");
            if (hasFlagMethod == null) {
                throw new NullReferenceException();
            }
            return values.Where(e => !e.Equals(default(TEnum)) && hasFlagMethod.Invoke(options, new object[] { e }).To<bool>())
                         .ToList();
        }

        /// <summary>
        /// Gets the associated attributes of the values of an enum
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum</typeparam>
        /// <typeparam name="TAttribute">The type of the attribute to retrieve</typeparam>
        /// <returns>The dictionary of associated attributes of the values of an enum</returns>
        public static Dictionary<TEnum, TAttribute> GetAttributes<TEnum, TAttribute>() where TEnum : Enum where TAttribute : Attribute {
            var type = typeof(TEnum);
            var typeInformation = type.GetTypeInformation();
            if (!typeInformation.IsEnum) {
                throw new InvalidEnumArgumentException();
            }
            var values = Enum.GetValues(type);
            var result = new Dictionary<TEnum, TAttribute>();
            foreach (var value in values) {
                result.Add((TEnum) value, ((Enum) value).GetAttribute<TAttribute>());
            }
            return result;
        }

        public static TEnum ParseFromAttribute<TEnum, TAttribute, TProp>(TProp attributeValue, Func<TAttribute, TProp> attributePropertySelector) where TEnum : Enum where TAttribute : Attribute {
            var attributes = GetAttributes<TEnum, TAttribute>();
            foreach (var attrKvp in attributes) {
                var attrValue = attributePropertySelector(attrKvp.Value);
                if (attrValue.Equals(attributeValue)) {
                    return attrKvp.Key;
                }
            }
            return default(TEnum);
        }

        /// <summary>
        /// Returns an indication whether a constant with a specified value exists in a specified enumeration.
        /// </summary>
        /// <param name="enum">The enum</param>
        /// <returns>True if the enum is defined, false if not</returns>
        public static bool IsDefined(this Enum @enum) {
            var enumType = @enum.GetType();
            var typeInfo = enumType.GetTypeInformation();
            if (!typeInfo.IsEnum) {
                return false;
            }
            if (typeInfo.IsEnumWithFlag) {
                return IsDefined(enumType, @enum.To<long>());
            }
            return Enum.IsDefined(enumType, @enum);
        }

        public static bool IsDefined(object value, Type type) {
            if (!type.IsEnum) {
                return false;
            }
            var typeInfo = type.GetTypeInformation();
            if (typeInfo.IsEnumWithFlag) {
                return IsDefined(type, value.To<long>());
            }
            return Enum.IsDefined(type, value);
        }

        /// <summary>
        /// Checks if the <paramref name="value"/> is defined in a flag <see cref="Enum"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsDefined<T>(long value) where T : Enum {
            return IsDefined(typeof(T), value);
        }

        /// <summary>
        /// Checks if the <paramref name="value"/> is defined in a flag <see cref="Enum"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsDefined(Type type, long value) {
            var typeInfo = type.GetTypeInformation();
            return IsDefined(typeInfo, value); 
        }

        private static bool IsDefined(TypeInformation typeInformation, long value) {
            var type = typeInformation.Type;
            if (!type.IsEnum) {
                return false;
            }
            if (!typeInformation.IsEnumWithFlag) {
                return Enum.IsDefined(type, value);
            }
            var max = Enum.GetValues(type).Cast<object>().Select(v => v.To<long>()).Aggregate((e1, e2) => e1 | e2);
            return (max & value) == value;
        }
    }
}