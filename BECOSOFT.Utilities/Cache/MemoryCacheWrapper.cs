using System;
using System.Collections.Specialized;
using System.Runtime.Caching;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Helpers;

namespace BECOSOFT.Utilities.Cache {
    /// <inheritdoc />
    public sealed class MemoryCacheWrapper : IMemoryCacheWrapper {
        private MemoryCache _memoryCache;

        public void Initialize(string name) {
            _memoryCache = CreateCache(name);
        }

        private static MemoryCache CreateCache(string name) {
            var config = new NameValueCollection {
                {"CacheMemoryLimitMegabytes", "200"},
                {"physicalMemoryLimitPercentage", "49"},
                {"pollingInterval", "60:00:00"}
            };
            return new MemoryCache(name, config);
        }

        /// <inheritdoc />
        public string Name => _memoryCache.Name;

        /// <inheritdoc />
        public long CacheMemoryLimitInBytes => _memoryCache.CacheMemoryLimit;

        /// <inheritdoc />
        public long PhysicalMemoryLimit => _memoryCache.PhysicalMemoryLimit;

        /// <inheritdoc />
        public TimeSpan PollingInterval => _memoryCache.PollingInterval;

        /// <inheritdoc />
        public long IntervalInMilliseconds { get; set; } = TimeHelper.GetInterval(1, TimeInterval.Hour);

        /// <inheritdoc />
        public CacheItemPolicy CacheItemPolicy => new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(IntervalInMilliseconds) };

        /// <inheritdoc />
        public void AddOrUpdate(string key, object value) {
            _memoryCache.Set(key, new ObjectWrapper(value), CacheItemPolicy);
        }

        /// <inheritdoc />
        public bool TryGetValue<T>(string key, out T value) {
            return TryFunc(key, _memoryCache.Get, out value);
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out object value) {
            return TryFunc(key, _memoryCache.Get, out value);
        }

        /// <inheritdoc />
        public bool TryRemove<T>(string key, out T value) {
            return TryFunc(key, _memoryCache.Remove, out value);
        }

        /// <inheritdoc />
        public bool TryRemove(string key, out object value) {
            return TryFunc(key, _memoryCache.Remove, out value);
        }

        /// <inheritdoc />
        public void Remove(string key) {
            _memoryCache.Remove(key);
        }

        /// <inheritdoc />
        public bool ContainsKey(string key) {
            return _memoryCache.Contains(key);
        }

        /// <inheritdoc />
        public long Count => _memoryCache.GetCount();

        /// <inheritdoc />
        public void ClearCache() {
            var name = Name;
            _memoryCache?.Dispose();
            _memoryCache = CreateCache(name);
        }

        // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
        public TReturn Retrieve<TReturn>(string key, Func<TReturn> cacheMissAction) {
            if (key == null) {
                return default;
            }
            if (TryGetValue(key, out TReturn value)) {
                return value;
            }
            value = cacheMissAction();
            if (value == null) {
                value = default;
            }
            AddOrUpdate(key, value);
            return value;
        }

        private static bool TryFunc<T>(string key, Func<string, string, object> func, out T value) {
            var result = TryFunc(key, func, out var temp);
            if (temp is T castValue) {
                value = castValue;
            } else {
                var type = typeof(T);
                if (type.IsClass && type != typeof(string) || type.IsInterface) {
                    value = default;
                } else {
                    var converter = Converter.GetDelegate(typeof(T));
                    value = (T) converter(temp);
                }
            }
            return result;
        }

        private static bool TryFunc(string key, Func<string, string, object> func, out object value) {
            var result = false;
            var item = func(key, null);
            if (!(item is ObjectWrapper wrapper)) {
                value = default;
            } else {
                var wrapped = wrapper.WrappedObject;
                result = true;
                value = wrapped;
            }
            return result;
        }

        private class ObjectWrapper {
            public ObjectWrapper(object obj) {
                WrappedObject = obj;
            }

            public object WrappedObject { get; }
        }
    }
}