using System;
using System.Collections.Generic;
using System.Reflection;

namespace BECOSOFT.Data.Converters {
    /// <summary>
    /// Class for creating delegates
    /// </summary>
    public static class DelegateCreator {
        private static readonly MethodInfo GenericConvertEntity = GetGenericConvertEntityMethodInfo();
        internal delegate object Projector(object[] valueContainer, List<PropertyMapping> columnIndices);

        internal static Projector CreateConvertEntityDelegate(Type targetType) {
            var genericHelper = GenericConvertEntity.MakeGenericMethod(targetType);
            return (Projector) Delegate.CreateDelegate(typeof(Projector), genericHelper);
        }

        private static MethodInfo GetGenericConvertEntityMethodInfo() {
            return typeof(EntityConverter).GetMethod(nameof(EntityConverter.ConvertEntity), BindingFlags.Static | BindingFlags.NonPublic,
                                                     null, new[] { typeof(object[]), typeof(List<PropertyMapping>) }, null);
        }
    }
}
