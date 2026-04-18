using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BECOSOFT.Utilities.Extensions {
    /// <summary>
    /// Extensions for the <see cref="Type"/>-class
    /// </summary>
    public static class TypeExtensions {
        /// <summary>
        /// All the numeric-types
        /// </summary>
        public static readonly HashSet<Type> NumericTypes =
            new HashSet<Type>(new[] {
                typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint),
                typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)
            });

        /// <summary>
        /// All the signed int-based types
        /// </summary>
        public static readonly HashSet<Type> NumericSignedIntTypes =
            new HashSet<Type>(new[] {
                typeof(sbyte), typeof(short), typeof(int), typeof(long)
            });

        /// <summary>
        /// All the unsigned int-based types
        /// </summary>
        public static readonly HashSet<Type> NumericUnsignedIntTypes =
            new HashSet<Type>(new[] {
                typeof(byte), typeof(ushort), typeof(uint), typeof(ulong)
            });

        /// <summary>
        /// All the int-based types
        /// </summary>
        public static readonly HashSet<Type> NumericIntTypes =
            new HashSet<Type>(NumericSignedIntTypes.Union(NumericUnsignedIntTypes));

        /// <summary>
        /// All the decimal-based types
        /// </summary>
        public static readonly HashSet<Type> NumericDecimalTypes =
            new HashSet<Type>(new[] {
                typeof(float), typeof(double), typeof(decimal)
            });

        private static readonly ConcurrentDictionary<Type, TypeInformation> TypeInformation =
            new ConcurrentDictionary<Type, TypeInformation>();


        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, bool>> InterfaceImplementations =
            new ConcurrentDictionary<Type, ConcurrentDictionary<Type, bool>>();

        /// <summary>
        /// Gets the information about this type
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>The information</returns>
        public static TypeInformation GetTypeInformation(this Type type) {
            if (!TypeInformation.TryGetValue(type, out var typeInformation)) {
                typeInformation = new TypeInformation(type);
                TypeInformation.TryAdd(type, typeInformation);
            }
            return typeInformation;
        }

        /// <summary>
        /// Checks if a type is numeric
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>Value indicating whether the value is numeric</returns>
        public static bool IsNumeric(this Type type) {
            if (type.IsEnum) {
                return NumericTypes.Contains(type.GetEnumUnderlyingType());
            }
            return NumericTypes.Contains(type) || NumericTypes.Contains(Nullable.GetUnderlyingType(type));
        }

        /// <summary>
        /// Checks if a type is integer-based
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>Value indicating whether the value is integer-based</returns>
        public static bool IsInteger(this Type type) {
            if (type.IsEnum) {
                return NumericIntTypes.Contains(type.GetEnumUnderlyingType());
            }
            return NumericIntTypes.Contains(type) || NumericIntTypes.Contains(Nullable.GetUnderlyingType(type));
        }

        /// <summary>
        /// Checks if a type is signed integer-based
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>Value indacting whether the value is signed integer-based</returns>
        public static bool IsSignedInteger(this Type type) {
            if (type.IsEnum) {
                return NumericSignedIntTypes.Contains(type.GetEnumUnderlyingType());
            }
            return NumericSignedIntTypes.Contains(type) || NumericSignedIntTypes.Contains(Nullable.GetUnderlyingType(type));
        }

        /// <summary>
        /// Checks if a type is unsigned integer-based
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>Value indacting whether the value is unsigned integer-based</returns>
        public static bool IsUnsignedInteger(this Type type) {
            if (type.IsEnum) {
                return NumericUnsignedIntTypes.Contains(type.GetEnumUnderlyingType());
            }
            return NumericUnsignedIntTypes.Contains(type) || NumericUnsignedIntTypes.Contains(Nullable.GetUnderlyingType(type));
        }

        /// <summary>
        /// Checks if a type is decimal-based
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>Value indicating whether the value is decimal-based</returns>
        public static bool IsDecimal(this Type type) {
            if (type.IsEnum) {
                return NumericDecimalTypes.Contains(type.GetEnumUnderlyingType());
            }
            return NumericDecimalTypes.Contains(type) || NumericDecimalTypes.Contains(Nullable.GetUnderlyingType(type));
        }

        /// <summary>
        /// Checks if a type is a subclass of a raw generic
        /// </summary>
        /// <param name="sourceType">The source type</param>
        /// <param name="generic">The generic type</param>
        /// <returns>Value indicating whether the value is a subclass of the generic type</returns>
        public static bool IsSubclassOfRawGeneric(this Type sourceType, Type generic) {
            var temp = sourceType;
            while (temp != null && temp != typeof(object)) {
                var cur = temp.IsGenericType ? temp.GetGenericTypeDefinition() : sourceType;
                if (generic == cur) {
                    return true;
                }
                temp = temp.BaseType;
            }
            return false;
        }

        /// <summary>
        /// Returns the generic type (with defined type arguments) that implements <see cref="generic"/>.
        /// <para>
        /// Example:
        /// </para>
        /// <para>
        /// Generic&lt;T&gt; is a generic type
        /// </para>
        /// <para>
        /// ImplGeneric : Generic&lt;Test&gt; implements Generic&lt;T&gt; with generic type arguments Test
        /// </para>
        /// <para>
        /// This function returns this type: Generic&lt;Test&gt;
        /// </para>
        /// </summary>
        /// <param name="sourceType">The source type</param>
        /// <param name="generic">The generic type</param>
        /// <returns>Value indicating whether the value is a subclass of the generic type</returns>
        public static Type GetGenericTypeFromSubclassOfRawGeneric(this Type sourceType, Type generic) {
            while (sourceType != null && sourceType != typeof(object)) {
                var cur = sourceType.IsGenericType ? sourceType.GetGenericTypeDefinition() : sourceType;
                if (generic == cur) {
                    return sourceType;
                }
                sourceType = sourceType.BaseType;
            }
            return null;
        }

        /// <summary>
        /// Gets the value of a static method
        /// </summary>
        /// <param name="type">The type to get the method from</param>
        /// <param name="methodName">The name of the method</param>
        /// <returns>The value of the static method</returns>
        public static object GetStaticMethodValue(this Type type, string methodName) {
            return type.GetProperty(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                       ?.GetValue(null);
        }

        /// <summary>
        /// Checks if a type is a generic list
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>Value indicating whether the value is a generic list</returns>
        public static bool IsGenericList(this Type type) {
            if (type == null) {
                throw new ArgumentException();
            }
            if (type == typeof(string)) {
                return false;
            }
            if (type.IsArray) {
                return false;
            }

            return type.GetInterfaces()
                       .Where(@interface => @interface.IsGenericType)
                       .Any(@interface => @interface.GetGenericTypeDefinition() == typeof(ICollection<>) || @interface.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        /// <summary>
        /// Checks if a type has a default constructor
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>Value indicating whether the type has a default constructor</returns>
        public static bool HasDefaultConstructor(this Type type) {
            return type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null;
        }

        /// <summary>
        /// Get the subclasses of the provided <see cref="type"/>.
        /// </summary>
        /// <param name="type">The type to find the subclasses for.</param>
        /// <returns></returns>
        public static List<Type> GetSubclasses(this Type type) {
            return Assembly.GetAssembly(type).GetTypes().Where(t => t.IsSubclassOf(type)).ToList();
        }

        public static bool IsCompatibleWith(this Type source, Type target) {
            if (source == target) { return true; }
            if (!target.IsValueType) { return target.IsAssignableFrom(source); }
            var st = GetNonNullableType(source);
            var tt = GetNonNullableType(target);
            if (st != source && tt == target) { return false; }
            var sc = st.IsEnum ? TypeCode.Object : Type.GetTypeCode(st);
            var tc = tt.IsEnum ? TypeCode.Object : Type.GetTypeCode(tt);
            switch (sc) {
                case TypeCode.SByte:
                    switch (tc) {
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Byte:
                    switch (tc) {
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Int16:
                    switch (tc) {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.UInt16:
                    switch (tc) {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Int32:
                    switch (tc) {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.UInt32:
                    switch (tc) {
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Int64:
                    switch (tc) {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.UInt64:
                    switch (tc) {
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Single:
                    switch (tc) {
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return true;
                    }
                    break;
                default:
                    if (st == tt) {
                        return true;
                    }
                    break;
            }
            return false;
        }

        public static Type GetNonNullableType(this Type type) {
            return type.IsNullableType() ? type.GetGenericArguments()[0] : type;
        }

        public static bool IsNullableType(this Type type) {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsEnumType(this Type type) {
            return type.GetNonNullableType().IsEnum;
        }

        public static string GetTypeName(this Type type) {
            var baseType = type.GetNonNullableType();
            var s = baseType.Name;
            if (type != baseType) {
                s += '?';
            }
            return s;
        }

        public static string GetGenericName(this Type type, bool includeNameSpace = true) {
            var typeName = (includeNameSpace ? type.Namespace + "." : "") + type.Name;
            var args = type.GetGenericArguments();
            if (args.IsEmpty()) {
                return typeName;
            }
            typeName = typeName.Remove(typeName.LastIndexOf("`", StringComparison.Ordinal));
            return $"{typeName}<{string.Join(", ", args.Select(a => a.GetGenericName(includeNameSpace)))}>";
        }

        public static string GetNameWithoutGenerics(this Type t) {
            var name = t.Name;
            var index = name.IndexOf('`');
            return index == -1 ? name : name.Substring(0, index);
        }

        public static bool IsInterfaceImplementationOf<T>(this Type type) {
            return type.IsInterfaceImplementationOf(typeof(T));
        }

        public static bool IsInterfaceImplementationOf(this Type type, Type interfaceType) {
            var currentInterfaceImplementations = InterfaceImplementations.TryGetValueWithDefault(interfaceType);
            if (currentInterfaceImplementations == null) {
                currentInterfaceImplementations = new ConcurrentDictionary<Type, bool>();
                InterfaceImplementations.TryAdd(interfaceType, currentInterfaceImplementations);
            }

            if (!currentInterfaceImplementations.TryGetValue(type, out var isImplementation)) {
                isImplementation = type.GetInterfaces().Contains(interfaceType);
                currentInterfaceImplementations.TryAdd(type, isImplementation);
            }

            return isImplementation;
        }


        //Based on https://stackoverflow.com/a/2742288/4182837
        public static bool IsSameOrSubclassOf(this Type type, Type possibleBaseType) {
            if (type == null || possibleBaseType == null) {
                return false;
            }

            return type == possibleBaseType || type.IsSubclassOf(possibleBaseType);
        }
    }
}