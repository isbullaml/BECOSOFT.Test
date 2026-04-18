using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace BECOSOFT.Web.Helpers {

    /// <summary>
    /// source: https://stackoverflow.com/questions/41167770/mvc-5-increase-max-json-length-in-post-request
    /// </summary>
    public class MaxSizeJsonValueProviderFactory : ValueProviderFactory {

        private static void AddToBackingStore(EntryLimitedDictionary backingStore, string prefix, object value) {
            if (value is IDictionary<string, object> dictionary) {
                foreach (var current in dictionary) {
                    AddToBackingStore(backingStore, MakePropertyKey(prefix, current.Key), current.Value);
                }
                return;
            }
            if (value is IList list) {
                for (var i = 0; i < list.Count; i++) {
                    AddToBackingStore(backingStore, MakeArrayKey(prefix, i), list[i]);
                }
                return;
            }
            backingStore.Add(prefix, value);
        }

        private static object GetDeserializedObject(ControllerContext controllerContext) {
            if (!controllerContext.HttpContext.Request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase)) {
                return null;
            }
            var streamReader = new StreamReader(controllerContext.HttpContext.Request.InputStream);
            var text = streamReader.ReadToEnd();
            if (string.IsNullOrEmpty(text)) {
                return null;
            }
            var javaScriptSerializer = new JavaScriptSerializer();
            // To solve this problem:
            javaScriptSerializer.MaxJsonLength = int.MaxValue;
            // ----------------------------------------
            return javaScriptSerializer.DeserializeObject(text);
        }

        public override IValueProvider GetValueProvider(ControllerContext controllerContext) {
            if (controllerContext == null) {
                throw new ArgumentNullException(nameof(controllerContext));
            }
            var deserializedObject = GetDeserializedObject(controllerContext);
            if (deserializedObject == null) {
                return null;
            }
            var dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var backingStore = new EntryLimitedDictionary(dictionary);
            AddToBackingStore(backingStore, string.Empty, deserializedObject);
            return new DictionaryValueProvider<object>(dictionary, CultureInfo.CurrentCulture);
        }

        private static string MakeArrayKey(string prefix, int index) {
            return prefix + "[" + index.ToString(CultureInfo.InvariantCulture) + "]";
        }

        private static string MakePropertyKey(string prefix, string propertyName) {
            if (!string.IsNullOrEmpty(prefix)) {
                return prefix + "." + propertyName;
            }
            return propertyName;
        }
        private class EntryLimitedDictionary {
            private static readonly int MaximumDepth = GetMaximumDepth();
            private readonly IDictionary<string, object> _innerDictionary;
            private int _itemCount;

            public EntryLimitedDictionary(IDictionary<string, object> innerDictionary) {
                _innerDictionary = innerDictionary;
            }

            public void Add(string key, object value) {
                if (++_itemCount > MaximumDepth) {
                    //throw new InvalidOperationException(MvcResources.JsonValueProviderFactory_RequestTooLarge);
                    throw new InvalidOperationException("itemCount is over maximumDepth");
                }
                _innerDictionary.Add(key, value);
            }

            private static int GetMaximumDepth() {
                var appSettings = ConfigurationManager.AppSettings;
                if (appSettings == null) {
                    return 1000;
                }
                var values = appSettings.GetValues("aspnet:MaxJsonDeserializerMembers");
                if (values != null && values.Length > 0 && int.TryParse(values[0], out var result)) {
                    return result;
                }
                return 1000;
            }
        }
    }
}