using BECOSOFT.Utilities.Attributes;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Helpers {
    internal static class CodeAttributeContainer<TEnum> where TEnum : struct, Enum {
        private static readonly Dictionary<string, TEnum> FromCodeDictionary;
        private static readonly Dictionary<TEnum, string> ToCodeDictionary;

        static CodeAttributeContainer() {
            var values = EnumHelper.GetAttributes<TEnum, CodeAttribute>();
            ToCodeDictionary = values.ToDictionary(e => e.Key, e => e.Value?.Description);
            FromCodeDictionary = values.Where(e => e.Value != null).GroupBy(e => e.Value.Description.ToLower()).ToDictionary(g => g.Key, g => g.OrderBy(e => e.Key).First().Key);
        }

        public static string GetCode(TEnum enumValue) {
            return ToCodeDictionary.TryGetValueWithDefault(enumValue);
        }

        public static string GetCodeNullable(TEnum? enumValue) {
            if (!enumValue.HasValue) { return null; }
            return ToCodeDictionary.TryGetValueWithDefault(enumValue.Value);
        }

        public static TEnum FromCode(string code) {
            if (code.IsNullOrWhiteSpace()) {
                return (TEnum) (object) 0;
            }
            return FromCodeDictionary.TryGetValueWithDefault(code.ToLower());
        }

        public static TEnum? FromCodeNullable(string code) {
            if (code.IsNullOrWhiteSpace()) {
                return null;
            }
            var lowerCode = code.ToLower();
            TEnum value;
            if (FromCodeDictionary.TryGetValue(lowerCode, out value)) {
                return value;
            }
            return null;
        }
    }
}