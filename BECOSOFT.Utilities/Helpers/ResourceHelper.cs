using BECOSOFT.Utilities.Annotations;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace BECOSOFT.Utilities.Helpers {
    public static class ResourceHelper {
        private static readonly List<Type> _resourceTypes = new List<Type>();
        public static IReadOnlyList<Type> ResourceTypes => _resourceTypes.AsReadOnly();

        public static TResourceManager GetResourceManager<TResourceManager>(Type resourcesType) where TResourceManager : ResourceManager {
            if (!resourcesType.Name.Contains("Resources", StringComparison.InvariantCultureIgnoreCase)) {
                return null;
            }
            var resourceManagerProp = resourcesType.GetProperty("ResourceManager", BindingFlags.Static | BindingFlags.Public);
            if (resourceManagerProp == null) {
                return null;
            }
            var obj = resourceManagerProp.GetValue(null);
            return obj as TResourceManager;
        }

        public static List<string> GetResourceKeys(Type resourcesType) {
            if (!resourcesType.Name.Contains("Resources", StringComparison.InvariantCultureIgnoreCase)) {
                return new List<string>(0);
            }
            var result = new List<string>();
            var properties = resourcesType.GetProperties(BindingFlags.Public | BindingFlags.Static);
            var stringType = typeof(string);
            foreach (var propertyInfo in properties) {
                if (propertyInfo.PropertyType != stringType) { continue; }
                result.Add(propertyInfo.Name);
            }
            return result;
        }

        public static void SetResourceManager<TResourceManager>(HashSet<OverruledResource> overruledResources, Type resourcesType) where TResourceManager : ResourceManager {
            if (!resourcesType.Name.Contains("Resources", StringComparison.InvariantCultureIgnoreCase)) {
                return;
            }
            var resourceManagerProp = resourcesType.GetProperty("ResourceManager", BindingFlags.Static | BindingFlags.Public);
            if (resourceManagerProp == null) {
                return;
            }
            var resourceManagerField = resourcesType.GetField("resourceMan", BindingFlags.NonPublic | BindingFlags.Static);
            if (resourceManagerField == null) {
                return;
            }
            var mainAssemblyProp = resourceManagerProp.PropertyType.GetField("MainAssembly", BindingFlags.Instance | BindingFlags.NonPublic);
            var baseNameFieldProp = resourceManagerProp.PropertyType.GetField("BaseNameField", BindingFlags.Instance | BindingFlags.NonPublic);
            if (mainAssemblyProp == null || baseNameFieldProp == null) {
                return;
            }
            var currentResourceManager = resourceManagerProp.GetValue(null);
            var newResourceManagerType = typeof(TResourceManager);
            var constructors = newResourceManagerType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            var constructor = constructors.FirstOrDefault(ci => ci.GetParameters().Length == 3 && ci.GetParameters().Any(p => p.ParameterType == typeof(HashSet<OverruledResource>)));
            if (constructor == null) {
                return;
            }
            var mainAssembly = mainAssemblyProp.GetValue(currentResourceManager);
            var baseNameField = baseNameFieldProp.GetValue(currentResourceManager);
            var obj = constructor.Invoke(new[] { baseNameField, mainAssembly, overruledResources });
            var newResourceManager = obj as TResourceManager;
            resourceManagerField.SetValue(null, newResourceManager);
            _resourceTypes.Add(resourcesType);
        }

        public static void SetResourceManager<T, TResourceManager>(HashSet<OverruledResource> overruledResources) where TResourceManager : ResourceManager {
            SetResourceManager<TResourceManager>(overruledResources, typeof(T));
        }

        public static void SetResourceManager<TResources>(ResourceManager resourceManager) {
            var resourcesType = typeof(TResources);
            if (!resourcesType.Name.Contains("Resources", StringComparison.InvariantCultureIgnoreCase)) {
                return;
            }
            var resourceManagerProp = resourcesType.GetProperty("ResourceManager", BindingFlags.Static | BindingFlags.Public);
            if (resourceManagerProp == null) {
                return;
            }
            var resourceManagerField = resourcesType.GetField("resourceMan", BindingFlags.NonPublic | BindingFlags.Static);
            if (resourceManagerField == null) {
                return;
            }
            resourceManagerField.SetValue(null, resourceManager);
        }
    }

    public class ClientSpecificResourceManager : ResourceManager {
        private static Dictionary<string, OverruledResource> _overruledResources;
        public static bool HasOverruledResource { get; private set; }

        public ClientSpecificResourceManager([NotNull] string baseName, [NotNull] Assembly assembly, HashSet<OverruledResource> overruledResources)
            : base(baseName, assembly) {
            UpdateOverruledResources(overruledResources);
        }

        public static void UpdateOverruledResources(HashSet<OverruledResource> overruledResources) {
            _overruledResources = overruledResources?.ToDictionary(or => or.ResourceKey);
            HasOverruledResource = _overruledResources.HasAny();
        }

        public string GetStringWithoutOverruled(string name) {
            return base.GetString(name);
        }


        public string GetStringWithoutOverruled(string name, CultureInfo culture) {
            return base.GetString(name, culture);
        }


        public override string GetString(string name) {
            if (GetOverruledResource(name, null, out var overruledResource)) {
                return overruledResource;
            }
            return base.GetString(name);
        }

        public override string GetString(string name, CultureInfo culture) {
            if (GetOverruledResource(name, culture, out var overruledResource)) {
                return overruledResource;
            }
            return base.GetString(name, culture);
        }

        private bool GetOverruledResource(string name, CultureInfo culture, out string overruledResource) {
            if (!HasOverruledResource) {
                overruledResource = null;
                return false;
            }
            if (_overruledResources.TryGetValue(name, out var or) && or.HasResources) {
                culture = culture ?? Thread.CurrentThread.CurrentUICulture;
                var resource = or.GetResource(culture);
                if (!resource.IsNullOrWhiteSpace()) {
                    overruledResource = resource;
                    return true;
                }
            }
            overruledResource = null;
            return false;
        }
    }

    public class OverruledResource : IEquatable<OverruledResource> {
        private readonly Dictionary<CultureInfo, string> _resources;
        public string ResourceKey { get; }
        public bool HasResources { get; }

        public OverruledResource(string resourceKey, Dictionary<CultureInfo, string> resources) {
            ResourceKey = resourceKey;
            _resources = resources;
            HasResources = resources.HasAny();
        }

        public string GetResource(CultureInfo cultureInfo) {
            return _resources.TryGetValueWithDefault(cultureInfo);
        }

        public bool Equals(OverruledResource other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }
            return ResourceKey == other.ResourceKey;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            if (ReferenceEquals(this, obj)) {
                return true;
            }
            if (obj.GetType() != this.GetType()) {
                return false;
            }
            return Equals((OverruledResource)obj);
        }

        public override int GetHashCode() {
            return (ResourceKey != null ? ResourceKey.GetHashCode() : 0);
        }

        public static bool operator ==(OverruledResource left, OverruledResource right) {
            return Equals(left, right);
        }

        public static bool operator !=(OverruledResource left, OverruledResource right) {
            return !Equals(left, right);
        }
    }
}