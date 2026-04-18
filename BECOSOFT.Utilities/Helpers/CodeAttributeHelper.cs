using System;

namespace BECOSOFT.Utilities.Helpers {
    public class CodeAttributeHelper {
        public static string GetCode<TEnum>(TEnum value) where TEnum : struct, Enum {
            return CodeAttributeContainer<TEnum>.GetCode(value);
        }

        public static string GetCode<TEnum>(TEnum? value) where TEnum : struct, Enum {
            return CodeAttributeContainer<TEnum>.GetCodeNullable(value);
        }

        public static TEnum FromCode<TEnum>(string value) where TEnum : struct, Enum {
            return CodeAttributeContainer<TEnum>.FromCode(value);
        }

        public static TEnum? FromCodeNullable<TEnum>(string value) where TEnum : struct, Enum {
            return CodeAttributeContainer<TEnum>.FromCodeNullable(value);
        }
    }
}