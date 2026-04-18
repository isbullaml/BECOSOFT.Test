using System;

namespace BECOSOFT.Utilities.Converters {
    public static class ConverterExtensions {

        /// <summary>
        /// Converts a <see cref="bool"/> value to the specified <see cref="T"/> type.
        /// </summary>
        /// <typeparam name="T">Target conversion type</typeparam>
        /// <param name="value">A <see cref="bool"/> value</param>
        /// <returns>A converted value</returns>
        public static T To<T>(this bool value) {
            return Converter.GetValue<T>(value);
        }

        /// <summary>
        /// Converts a <see cref="bool"/> value to the specified <see cref="T"/> type.
        /// </summary>
        /// <typeparam name="T">Target conversion type</typeparam>
        /// <param name="value">A <see cref="bool"/> value</param>
        /// <returns>A converted value</returns>
        public static T To<T>(this char value) {
            return Converter.GetValue<T>(value);
        }

        /// <summary>
        /// Converts a <see cref="sbyte"/> value to the specified <see cref="T"/> type.
        /// </summary>
        /// <typeparam name="T">Target conversion type</typeparam>
        /// <param name="value">A <see cref="sbyte"/> value</param>
        /// <returns>A converted value</returns>
        public static T To<T>(this sbyte value) {
            return Converter.GetValue<T>(value);
        }

        /// <summary>
        /// Converts a <see cref="byte"/> value to the specified <see cref="T"/> type.
        /// </summary>
        /// <typeparam name="T">Target conversion type</typeparam>
        /// <param name="value">A <see cref="byte"/> value</param>
        /// <returns>A converted value</returns>
        public static T To<T>(this byte value) {
            return Converter.GetValue<T>(value);
        }

        /// <summary>
        /// Converts a <see cref="short"/> value to the specified <see cref="T"/> type.
        /// </summary>
        /// <typeparam name="T">Target conversion type</typeparam>
        /// <param name="value">A <see cref="short"/> value</param>
        /// <returns>A converted value</returns>
        public static T To<T>(this short value) {
            return Converter.GetValue<T>(value);
        }

        /// <summary>
        /// Converts an <see cref="ushort"/> value to the specified <see cref="T"/> type.
        /// </summary>
        /// <typeparam name="T">Target conversion type</typeparam>
        /// <param name="value">A <see cref="ushort"/> value</param>
        /// <returns>A converted value</returns>
        public static T To<T>(this ushort value) {
            return Converter.GetValue<T>(value);
        }

        /// <summary>
        /// Converts a <see cref="int"/> value to the specified <see cref="T"/> type.
        /// </summary>
        /// <typeparam name="T">Target conversion type</typeparam>
        /// <param name="value">A <see cref="int"/> value</param>
        /// <returns>A converted value</returns>
        public static T To<T>(this int value) {
            return Converter.GetValue<T>(value);
        }

        /// <summary>
        /// Converts an <see cref="uint"/> value to the specified <see cref="T"/> type.
        /// </summary>
        /// <typeparam name="T">Target conversion type</typeparam>
        /// <param name="value">A <see cref="uint"/> value</param>
        /// <returns>A converted value</returns>
        public static T To<T>(this uint value) {
            return Converter.GetValue<T>(value);
        }

        /// <summary>
        /// Converts a <see cref="long"/> value to the specified <see cref="T"/> type.
        /// </summary>
        /// <typeparam name="T">Target conversion type</typeparam>
        /// <param name="value">A <see cref="long"/> value</param>
        /// <returns>A converted value</returns>
        public static T To<T>(this long value) {
            return Converter.GetValue<T>(value);
        }

        /// <summary>
        /// Converts an <see cref="ulong"/> value to the specified <see cref="T"/> type.
        /// </summary>
        /// <typeparam name="T">Target conversion type</typeparam>
        /// <param name="value">A <see cref="ulong"/> value</param>
        /// <returns>A converted value</returns>
        public static T To<T>(this ulong value) {
            return Converter.GetValue<T>(value);
        }

        /// <summary>
        /// Converts a <see cref="float"/> value to the specified <see cref="T"/> type.
        /// </summary>
        /// <typeparam name="T">Target conversion type</typeparam>
        /// <param name="value">A <see cref="float"/> value</param>
        /// <returns>A converted value</returns>
        public static T To<T>(this float value) {
            return Converter.GetValue<T>(value);
        }

        /// <summary>
        /// Converts a <see cref="double"/> value to the specified <see cref="T"/> type.
        /// </summary>
        /// <typeparam name="T">Target conversion type</typeparam>
        /// <param name="value">A <see cref="double"/> value</param>
        /// <returns>A converted value</returns>
        public static T To<T>(this double value) {
            return Converter.GetValue<T>(value);
        }

        /// <summary>
        /// Converts a <see cref="decimal"/> value to the specified <see cref="T"/> type.
        /// </summary>
        /// <typeparam name="T">Target conversion type</typeparam>
        /// <param name="value">A <see cref="decimal"/> value</param>
        /// <returns>A converted value</returns>
        public static T To<T>(this decimal value) {
            return Converter.GetValue<T>(value);
        }

        /// <summary>
        /// Converts a <see cref="Guid"/> value to the specified <see cref="T"/> type.
        /// </summary>
        /// <typeparam name="T">Target conversion type</typeparam>
        /// <param name="value">A <see cref="Guid"/> value</param>
        /// <returns>A converted value</returns>
        public static T To<T>(this Guid value) {
            return Converter.GetValue<T>(value);
        }

        /// <summary>
        /// Converts a <see cref="string"/> value to the specified <see cref="T"/> type.
        /// </summary>
        /// <typeparam name="T">Target conversion type</typeparam>
        /// <param name="value">A <see cref="string"/> value</param>
        /// <returns>A converted value</returns>
        public static T To<T>(this string value) {
            return Converter.GetValue<T>(value);
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> value to the specified <see cref="T"/> type.
        /// </summary>
        /// <typeparam name="T">Target conversion type</typeparam>
        /// <param name="value">A <see cref="DateTime"/> value</param>
        /// <returns>A converted value</returns>
        public static T To<T>(this DateTime value) {
            return Converter.GetValue<T>(value);
        }

        /// <summary>
        /// Converts an <see cref="object"/> value to the specified <see cref="T"/> type.
        /// </summary>
        /// <typeparam name="T">Target conversion type</typeparam>
        /// <param name="value">A <see cref="object"/> value</param>
        /// <returns>A converted value</returns>
        public static T To<T>(this object value) {
            return Converter.GetValue<T>(value);
        }

        public static bool ToBool<T>(this T value) {
            return value.To<bool>();
        }

        public static char ToChar<T>(this T value) {
            return value.To<char>();
        }

        public static byte ToByte<T>(this T value) {
            return value.To<byte>();
        }

        public static sbyte ToSByte<T>(this T value) {
            return value.To<sbyte>();
        }

        public static short ToShort<T>(this T value) {
            return value.To<short>();
        }

        public static ushort ToUShort<T>(this T value) {
            return value.To<ushort>();
        }

        public static int ToInt<T>(this T value) {
            return value.To<int>();
        }

        public static uint ToUInt<T>(this T value) {
            return value.To<uint>();
        }

        public static long ToLong<T>(this T value) {
            return value.To<long>();
        }

        public static ulong ToULong<T>(this T value) {
            return value.To<ulong>();
        }

        public static float ToSingle<T>(this T value) {
            return value.To<float>();
        }

        public static double ToDouble<T>(this T value) {
            return value.To<double>();
        }

        public static decimal ToDecimal<T>(this T value) {
            return value.To<decimal>();
        }

        public static Guid ToGuid<T>(this T value) {
            return value.To<Guid>();
        }

        public static DateTime ToDateTime<T>(this T value) {
            return value.To<DateTime>();
        }
    }
}
