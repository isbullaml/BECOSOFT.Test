using BECOSOFT.Utilities.Extensions.Collections;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BECOSOFT.Utilities.Extensions {
    /// <summary>
    /// Assembly extensions
    /// Source: https://stackoverflow.com/questions/26733/getting-all-types-that-implement-an-interface
    /// </summary>
    public static class AssemblyExtensions {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        public static List<Type> GetAllLoadableTypes(this Assembly assembly) {
            if (assembly == null) {
                throw new ArgumentNullException(nameof(assembly));
            }
            try {
                return assembly.GetTypes().ToList();
            } catch (ReflectionTypeLoadException e) {
                Logger.Error(e, $"Loading: {assembly.FullName}");
                if (e.LoaderExceptions.HasAny()) {
                    foreach (var loaderException in e.LoaderExceptions) {
                        Logger.Error(loaderException, $"Loading: {assembly.FullName}");
                    }
                }
                return e.Types.Where(t => t != null).ToList();
            } catch (Exception e) {
                Logger.Error(e, $"Loading: {assembly.FullName}");
                return new List<Type>(0);
            }
        }
    }
}
