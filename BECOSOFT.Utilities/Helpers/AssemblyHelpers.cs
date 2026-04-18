using BECOSOFT.Utilities.Annotations;
using BECOSOFT.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BECOSOFT.Utilities.Helpers {
    public static class AssemblyHelpers {
        /// <summary>
        /// Find all matching <see cref="Type"/> with name <see cref="typeName"/> by searching in the loaded assemblies of the <see cref="AppDomain.CurrentDomain"/>.
        /// </summary>
        /// <param name="typeName">Name of the <see cref="Type"/> to find</param>
        /// <returns></returns>
        public static Type[] FindLoadedTypes(string typeName) {
            var result = FindTypesByName(typeName);

            return result.ToArray();
        }

        /// <summary>
        /// Find the first matching <see cref="Type"/> with name <see cref="typeName"/> by searching the loaded assemblies of the <see cref="AppDomain.CurrentDomain"/>.
        /// </summary>
        /// <param name="typeName">Name of the <see cref="Type"/> to find</param>
        /// <returns></returns>
        [CanBeNull]
        public static Type FindLoadedType(string typeName) {
            var type = FindTypesByName(typeName).FirstOrDefault();
            return type;
        }

        public static Type FindTypeByExtensionMethod(string methodName, Type extendedType, Type[] argTypes = null) {
            var extensionAttributeType = typeof(ExtensionAttribute);
            var types = GetAllLoadableTypes().SelectMany(list => list.Where(t => t.IsSealed && !t.IsGenericType && !t.IsNested));
            foreach (var type in types) {
                var assemblyName = type.Assembly.FullName;
                var flags = BindingFlags.Static | BindingFlags.Public;
                if (assemblyName.StartsWith("BECOSOFT.")) {
                    flags |= BindingFlags.NonPublic;
                } else {
                    if (!type.IsVisible) { continue; }
                }
                var methods = type.GetMethods(flags);
                foreach (var method in methods) {
                    if (!method.Name.Equals(methodName)) { continue; }
                    if (!method.IsDefined(extensionAttributeType, false)) { continue; }
                    var parameters = method.GetParameters();
                    if (parameters[0].ParameterType != extendedType && !method.IsGenericMethod) { continue; }
                    if (argTypes == null) {
                        if (parameters.Length != 1) { continue; }
                        return type;
                    }
                    if (argTypes.Length + 1 != parameters.Length) { continue; }
                    var matchingTypes = true;
                    for (var i = 1; i < parameters.Length; i++) {
                        if (parameters[i].ParameterType != argTypes[i - 1]) {
                            matchingTypes = false;
                            break;
                        }
                    }
                    if (matchingTypes) {
                        return type;
                    }
                }
            }
            return null;
        }

        private static IEnumerable<Type> FindTypesByName(string typeName) {
            return GetAllLoadableTypes().SelectMany(types => types.Where(type => type.Name.Equals(typeName)));
        }

        private static IEnumerable<List<Type>> GetAllLoadableTypes() {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return assemblies.Where(a => !a.FullName.Contains("JetBrains", StringComparison.InvariantCultureIgnoreCase) 
                                         && !a.FullName.Contains("nunit", StringComparison.InvariantCultureIgnoreCase))
                             .Select(assembly => assembly.GetAllLoadableTypes());
        }
    }
}