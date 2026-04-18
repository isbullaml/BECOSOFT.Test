using BECOSOFT.Utilities.Helpers;
using System;

namespace BECOSOFT.Utilities.Converters {
    public static class EnumConverterHelper {
        public static U Convert<U, T>(T value) where T : struct {
            if (EnumHelper.IsDefined(value, typeof(T))) {
                return BoxingSafeConverter<T, U>.Instance(value);
            }
            return default;
        }

        public static Tuple<bool, U> TryParse<U>(string value) where U : struct {
            var didParse = Enum.TryParse(value, true, out U val);
            return Tuple.Create(didParse, val);
        }
    }
}