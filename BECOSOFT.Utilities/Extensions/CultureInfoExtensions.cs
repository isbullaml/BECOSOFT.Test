using BECOSOFT.Utilities.Extensions.Collections;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace BECOSOFT.Utilities.Extensions {
    /// <summary>
    /// Class for retrieving info about a culture
    /// </summary>
    public static class CultureInfoExtensions {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// The supported cultures
        /// </summary>
        public static readonly Dictionary<Type, HashSet<CultureInfo>> SupportedCultures = new Dictionary<Type, HashSet<CultureInfo>> { { typeof(Resources), LoadCulturesFromResource() } };

        /// <summary>
        /// Creates a <see cref="CultureInfo"/> from the provided <paramref name="name"/>. Optionally you can specify a <paramref name="defaultValue"/> when the culture cannot be created from the name.
        /// If <paramref name="defaultValue"/> is <see langword="null"/>, <see cref="CultureInfo"/>.<see cref="CultureInfo.InvariantCulture"/> will be used.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static CultureInfo CreateCultureWithFallback(string name, CultureInfo defaultValue = null) {
            try {
                if (name.IsNullOrWhiteSpace()) {
                    return defaultValue ?? CultureInfo.InvariantCulture;
                }
                var correctedName = name.Trim().EscapeEnterCharacters().RemoveControlCharacters().Replace("_", "-");
                return new CultureInfo(correctedName);
            } catch (Exception e) {
                Logger.Error(e);
                return defaultValue ?? CultureInfo.InvariantCulture;
            }
        }

        /// <summary>
        /// Retrieve a culture based on the name
        /// </summary>
        /// <param name="cultureName">The name of the culture</param>
        /// <param name="resourceType">The type for which the supported cultures should be searched. BECOSOFT.Utilities.<see cref="Resources"/> by default.</param>
        /// <returns>The info about the culture</returns>
        public static CultureInfo GetImplementedCulture(string cultureName, Type resourceType = null) {
            if (cultureName.IsNullOrWhiteSpace()) {
                return CultureInfo.InvariantCulture;
            }
            cultureName = cultureName.Replace("_", "-");
            var type = resourceType ?? typeof(Resources);
            var cultures = SupportedCultures.TryGetValueWithDefault(type);
            if (cultures == null) {
                cultures = LoadCulturesFromResource(type) ?? new HashSet<CultureInfo>();
            }
            foreach (var info in cultures) {
                if (info.Name.EqualsIgnoreCase(cultureName)) {
                    return info;
                }
                if (info.Name.Contains("-")) {
                    continue;
                }
                if (cultureName.StartsWith(info.Name.Split('-')[0])) {
                    return info;
                }
            }
            if (cultureName.IsNullOrWhiteSpace()) {
                return CultureInfo.InvariantCulture;
            }
            try {
                var culture = new CultureInfo(cultureName);
                var parentCulture = culture.Parent.Equals(CultureInfo.InvariantCulture) ? culture : culture.Parent;
                var cultureToUse = cultures.FirstOrDefault(sc => sc.Parent.Equals(parentCulture)) ?? CultureInfo.InvariantCulture;
                return cultureToUse;
            } catch (Exception e) {
                Logger.Error(e, "Error creating clture for '{0}' in resource type {1}.", cultureName, type.FullName);
            }
            return CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Source: https://stackoverflow.com/questions/553244/programmatic-way-to-get-all-the-available-languages-in-satellite-assemblies
        /// </summary>
        /// <returns></returns>
        public static HashSet<CultureInfo> LoadCulturesFromResource(Type resourceType = null) {
            var result = new HashSet<CultureInfo> {
                new CultureInfo("en-US") // default culture, is embedded in BECOSOFT.Core.dll
            };

            var type = resourceType ?? typeof(Resources);
            var resourceAssembly = type.Assembly;
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            var resourcePath = new UriBuilder(resourceAssembly.CodeBase).Path;
            var exeLocation = Path.GetDirectoryName(Uri.UnescapeDataString(resourcePath));
            if (exeLocation.IsNullOrWhiteSpace() || !Directory.Exists(exeLocation)) {
                exeLocation = Path.GetDirectoryName(resourceAssembly.Location);
                if (exeLocation.IsNullOrWhiteSpace() || !Directory.Exists(exeLocation)) {
                    Logger.Error("Did not find a resource path for '{0}', path: {1}", resourceAssembly.FullName, resourcePath);
                    throw new NullReferenceException(nameof(exeLocation));
                }
            }
            var resourceFileName = Path.GetFileNameWithoutExtension(resourceAssembly.Location) + ".resources.dll";
            foreach (var culture in cultures) {
                if (culture.Equals(CultureInfo.InvariantCulture)) {
                    continue; //do not use "==", won't work
                }
                var cultureDirectory = Path.Combine(exeLocation, culture.Name);
                var exists = Directory.Exists(cultureDirectory);
                if (!exists) { continue; }
                if (!File.Exists(Path.Combine(cultureDirectory, resourceFileName))) {
                    continue;
                }
                result.Add(culture);
            }
            if (resourceType != null) {
                SupportedCultures[type] = result;
            }
            return result;
        }
    }
}