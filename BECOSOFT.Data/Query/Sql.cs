using BECOSOFT.Data.Models;
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace BECOSOFT.Data.Query {
    /// <summary>
    /// Contains helper functions to generate SQL functions when translating a query with <see cref="QueryTranslator"/>.
    /// </summary>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used in QueryTranslator")]
    public static class Sql {
        /// <summary>
        /// Converts the parameters to an ISNULL SQL expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <param name="replacementValue"></param>
        /// <returns></returns>
        public static T IsNull<T>(T original, T replacementValue) => original;

        /// <summary>
        /// Converts the parameters to an NULLIF SQL expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <param name="replacementValue"></param>
        /// <returns></returns>
        public static T NullIf<T>(T original, T replacementValue) => original;

        /// <summary>
        /// Converts the parameters to an NULLIF SQL expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <param name="replacementValue"></param>
        /// <returns></returns>
        public static T? NullIf<T>(T? original, T? replacementValue) where T : struct => original;

        /// <summary>
        /// Converts the parameter to an LTRIM SQL expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <returns></returns>
        public static T LTrim<T>(T original)  => original;

        /// <summary>
        /// Converts the parameter to an RTRIM SQL expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <returns></returns>
        public static T RTrim<T>(T original) => original;

        /// <summary>
        /// Converts the parameter to an TRIM SQL expression, if the SQL server version supports this, otherwise it will fall back to LTRIM &amp; RTRIM.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <returns></returns>
        public static T Trim<T>(T original) => original;

        /// <summary>
        /// Converts the parameter to an LOWER SQL expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <returns></returns>
        public static T Lower<T>(T original) => original;

        /// <summary>
        /// Converts the parameter to an UPPER SQL expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <returns></returns>
        public static T Upper<T>(T original) => original;

        /// <summary>
        /// Converts the parameters to an LEN SQL expression
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static int Len(string original) => 0;

        /// <summary>
        /// Returns an integer representing the year part of a specified date.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Returns an integer representing the year part of a specified date.</returns>
        public static int Year(DateTime value) => 0;

        /// <summary>
        /// Returns an integer representing the month part of a specified date.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Returns an integer representing the month part of a specified date.</returns>
        public static int Month(DateTime value) => 0;

        /// <summary>
        /// Returns an integer representing the day part of the specified date.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Returns an integer representing the day part of the specified date.</returns>
        public static int Day(DateTime value) => 0;

        /// <summary>
        /// Returns an integer representing the specified datepart of the specified date.
        /// </summary>
        /// <param name="datePart"></param>
        /// <param name="value"></param>
        /// <returns>Returns an integer representing the specified datepart of the specified date.</returns>
        public static int DatePart(DatePart datePart, DateTime value) => 0;

        /// <summary>
        /// Returns the number of date or time datepart boundaries, crossed between two specified dates.
        /// </summary>
        /// <param name="datePart"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns>Returns the number of date or time datepart boundaries, crossed between two specified dates.</returns>
        public static int DateDiff(DatePart datePart, DateTime startDate, DateTime endDate) => 0;

        /// <summary>
        /// Returns the number of date or time datepart boundaries, crossed between two specified dates.
        /// </summary>
        /// <param name="datePart"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns>Returns the number of date or time datepart boundaries, crossed between two specified dates.</returns>
        public static long DateDiffBig(DatePart datePart, DateTime startDate, DateTime endDate) => 0;

        /// <summary>
        /// Casts the type of <see cref="value"/> to <see cref="SqlDbType"/>
        /// <param name="value"></param>
        /// <param name="sqlDbType">Parameter Type</param>
        /// </summary>
        /// <returns>Casts the type of <see cref="value"/> to <see cref="SqlDbType"/></returns>
        public static T Cast<T>(T value, SqlDbType sqlDbType) => value;

        /// <summary>
        /// Casts the type of <see cref="value"/> to <see cref="SqlDbType"/>
        /// <param name="value"></param>
        /// <param name="sqlDbType">Parameter Type</param>
        /// </summary>
        /// <returns>Casts the type of <see cref="value"/> to <see cref="SqlDbType"/></returns>
        public static TCast Cast<T, TCast>(T value, SqlDbType sqlDbType) => default(TCast);

        /// <summary>
        /// Casts the type of <see cref="value"/> to <see cref="SqlDbType"/>
        /// <param name="value"></param>
        /// <param name="sqlDbType">Parameter Type</param>
        /// <param name="size">Optional size definition (for example: NVARCHAR(4000)). Size -1 defines MAX</param>
        /// </summary>
        /// <returns>Casts the type of <see cref="value"/> to <see cref="SqlDbType"/></returns>
        public static TCast Cast<T, TCast>(T value, SqlDbType sqlDbType, int size) => default(TCast);

        /// <summary>
        /// Casts the type of <see cref="value"/> to <see cref="SqlDbType"/>
        /// <param name="value"></param>
        /// <param name="sqlDbType">Parameter Type</param>
        /// <param name="size">Optional size definition (for example: NVARCHAR(4000)). Size -1 defines MAX</param>
        /// </summary>
        /// <returns>Casts the type of <see cref="value"/> to <see cref="SqlDbType"/></returns>
        public static T Cast<T>(T value, SqlDbType sqlDbType, int size) => value;

        /// <summary>
        /// Casts the type of <see cref="value"/> to <see cref="SqlDbType"/>
        /// <param name="value"></param>
        /// <param name="sqlDbType">Parameter Type</param>
        /// <param name="precision">Optional size definition (for example: the 18 in NUMERIC(18,2))</param>
        /// <param name="scale">Optional scale definition (for example: the 2 in NUMERIC(18,2))</param>
        /// </summary>
        /// <returns>Casts the type of <see cref="value"/> to <see cref="SqlDbType"/></returns>
        public static T Cast<T>(T value, SqlDbType sqlDbType, int precision, int scale) => value;

        /// <summary>
        /// Converts this to RIGHT(<see cref="value"/>, <see cref="totalWidth"/>)
        /// </summary>
        /// <param name="value">value to pad</param>
        /// <param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.</param>
        public static string Left(string value, int totalWidth) => value;

        /// <summary>
        /// Converts this to LEFT(<see cref="value"/>, <see cref="totalWidth"/>)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.</param>
        public static string Right(string value, int totalWidth) => value;

        /// <summary>
        /// Returns a new string that right-aligns the characters in this instance by padding them on the left with a specified Unicode character, for a specified total length.
        /// <para>
        /// Converts this to RIGHT(REPLICATE(<see cref="paddingChar"/>, <see cref="totalWidth"/>) + <see cref="value"/>, <see cref="totalWidth"/>)
        /// </para>
        /// </summary>
        /// <param name="value">value to pad</param>
        /// <param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.</param>
        /// <param name="paddingChar">A Unicode padding character.</param>
        /// <returns>A new string that is equivalent to this instance, but right-aligned and padded on the left with as many <paramref name="paddingChar" /> characters as needed to create a length of <paramref name="totalWidth" />. However, if <paramref name="totalWidth" /> is less than the length of this instance, the method returns a reference to the existing instance. If <paramref name="totalWidth" /> is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
        public static string PadLeft(string value, int totalWidth, char paddingChar) => value;

        /// <summary>
        /// Returns a new string that left-aligns the characters in this string by padding them on the right with a specified Unicode character, for a specified total length.
        /// <para>
        /// Converts this to LEFT(<see cref="value"/> + REPLICATE(<see cref="paddingChar"/>, <see cref="totalWidth"/>), <see cref="totalWidth"/>)
        /// </para>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.</param>
        /// <param name="paddingChar">A Unicode padding character.</param>
        /// <returns>A new string that is equivalent to this instance, but left-aligned and padded on the right with as many <paramref name="paddingChar" /> characters as needed to create a length of <paramref name="totalWidth" />. However, if <paramref name="totalWidth" /> is less than the length of this instance, the method returns a reference to the existing instance. If <paramref name="totalWidth" /> is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
        public static string PadRight(string value, int totalWidth, char paddingChar) => value;
    }
}