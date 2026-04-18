using BECOSOFT.Data.Attributes;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Models;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace BECOSOFT.Data.Extensions {
    public static class EntityExtensions {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> TypeDictionary = new ConcurrentDictionary<Type, PropertyInfo[]>();
        private static readonly ConcurrentDictionary<PropertyInfo, IgnoreCopyAttribute> IgnoredPropertiesDictionary = new ConcurrentDictionary<PropertyInfo, IgnoreCopyAttribute>();
        private static readonly ConcurrentDictionary<Type, (MethodInfo, TypeInformation)> ListTypeInformation = new ConcurrentDictionary<Type, (MethodInfo, TypeInformation)>();
        /// <summary>
        /// Creates a new object of type <see cref="T"/> and copies all properties from the source object (<paramref name="source"/>).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T CopyObject<T>(this T source) where T: IEntity {
            var type = typeof(T);
            if (!type.HasDefaultConstructor()) {
                throw new ArgumentException("Type '{0}' does not have a default (parameterless) constructor".FormatWith(type));
            }

            var target = (T)CopyObject(source, type, false);
            return target;
        }
        /// <summary>
        /// Creates a new object of type <see cref="T"/> and copies all properties from the source object (<paramref name="source"/>) except for the <see cref="BaseEntity.Id"/>-property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T CopyObjectWithoutIdentities<T>(this T source) where T: BaseEntity {
            var type = typeof(T);
            if (!type.HasDefaultConstructor()) {
                throw new ArgumentException("Type '{0}' does not have a default (parameterless) constructor".FormatWith(type));
            }

            var target = (T)CopyObject(source, type, true);
            return target;
        }

        private static object CopyObject(object source, Type type, bool withoutIdentities) {
            if (!type.HasDefaultConstructor()) {
                throw new ArgumentException("Type '{0}' does not have a default (parameterless) constructor".FormatWith(type));
            }
            if (source == null) {
                return null;
            }
            var target = TypeActivator.CreateInstance(type);
            var interfaceType = typeof(IEntity);
            var baseChildType = typeof(BaseChild);
            var baseEntityType = typeof(BaseEntity);
            var properties = GetProperties(type);
            var isBaseEntityType = baseEntityType.IsAssignableFrom(type);
            foreach (var property in properties) {
                if (!property.CanWrite) { continue; }
                if (withoutIdentities && isBaseEntityType && property.Name == "Id") {
                    continue;
                }
                IgnoredPropertiesDictionary.TryGetValue(property, out var ignoreCopy);
                object value;
                if (ignoreCopy != null) {
                    if (ignoreCopy.CallConstructor) {
                        value = TypeActivator.CreateInstance(property.PropertyType);
                    } else {
                        var defaultDelegate = Converter.GetDelegate(property.PropertyType);
                        value = defaultDelegate(null);
                    }
                } else {
                    value = property.GetValue(source);
                }
                var propTypeInfo = property.PropertyType.GetTypeInformation();
                if (property.PropertyType.IsArray) {
                    property.SetValue(target, ((Array)value)?.Clone());
                } else if (property.PropertyType.IsGenericList()) {
                    var info = GetListTypeInfo(property);
                    var newList = TypeActivator.CreateInstance(property.PropertyType);
                    if (value != null) {
                        if (interfaceType.IsAssignableFrom(info.Item2.Type)) {
                            foreach (var item in (IEnumerable)value) {
                                var copiedItem = CopyObject(item, info.Item2.Type, withoutIdentities);
                                info.Item1.Invoke(newList, new[] { copiedItem });
                            }

                        } else if (CanCopy(info.Item2)) {
                            foreach (var item in (IEnumerable)value) {
                                info.Item1.Invoke(newList, new[] { item });
                            }
                        } else {
                            throw new ArgumentException("'{0}' on {1} could not be copied".FormatWith(property.PropertyType, type));
                        }
                    }
                    property.SetValue(target, newList);
                } else if (interfaceType.IsAssignableFrom(property.PropertyType) || baseChildType.IsAssignableFrom(property.PropertyType)) {
                    property.SetValue(target, CopyObject(value, value?.GetType() ?? property.PropertyType, withoutIdentities));
                } else if(CanCopy(propTypeInfo)) {
                    property.SetValue(target, value);
                } else {
                    throw new ArgumentException("'{0}' on {1} could not be copied".FormatWith(property.PropertyType, type));
                }
            }
            return target;
        }

        private static (MethodInfo, TypeInformation) GetListTypeInfo(PropertyInfo property) {
            if (!ListTypeInformation.TryGetValue(property.PropertyType, out var info)) {
                var listSourceType = property.PropertyType.GetGenericArguments()[0];
                info = (property.PropertyType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance), listSourceType.GetTypeInformation());
                ListTypeInformation.TryAdd(property.PropertyType, info);
            }
            return info;
        }

        private static PropertyInfo[] GetProperties(Type type) {
            if (!TypeDictionary.TryGetValue(type, out var properties)) {
                properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var propertyInfo in properties) {
                    var ignoreCopyAttr = propertyInfo.GetCustomAttribute<IgnoreCopyAttribute>();
                    if (ignoreCopyAttr == null) { continue; }
                    IgnoredPropertiesDictionary.TryAdd(propertyInfo, ignoreCopyAttr);
                }
                TypeDictionary.TryAdd(type, properties);
            }
            return properties;
        }

        private static bool CanCopy(TypeInformation typeInfo) {
            return typeInfo.IsPrimitive
                   || typeInfo.IsEnum
                   || typeInfo.IsString
                   || typeInfo.IsDateTime
                   || typeInfo.IsNullableOf
                   || typeInfo.IsNumeric;
        }
    }
}
