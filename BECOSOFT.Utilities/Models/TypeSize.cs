using System;
using System.Reflection.Emit;

// ReSharper disable StaticMemberInGenericType

namespace BECOSOFT.Utilities.Models {
    /// <summary>
    /// Generic static class containing the <see cref="Size"/> property.
    /// </summary>
    /// <typeparam name="T">Struct type</typeparam>
    public static class TypeSize<T> where T : struct {
        /// <summary>
        /// Returns the size of the struct in bytes.
        /// </summary>
        public static readonly int Size;

        static TypeSize() {
            var dm = new DynamicMethod("SizeOfType", typeof(int), new Type[] { });
            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Sizeof, typeof(T));
            il.Emit(OpCodes.Ret);
            Size = (int) dm.Invoke(null, null);
        }
    }
}