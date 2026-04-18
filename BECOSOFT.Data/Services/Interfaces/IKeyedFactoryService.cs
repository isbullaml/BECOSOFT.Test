using Autofac.Features.Indexed;
using System;

namespace BECOSOFT.Data.Services.Interfaces {
    public interface IKeyedFactoryService<out T> where T : Enum {
        T Type { get; }
    }
    public interface IFactoryService<in TKey, out T> where T : IKeyedFactoryService<TKey> where TKey : Enum {
        T GetInstance(TKey key);
    }

    public class FactoryService<TKey, T> : IFactoryService<TKey, T> where T : class, IKeyedFactoryService<TKey> where TKey : Enum {
        private readonly IIndex<TKey, T> _serviceImplementations;

        internal FactoryService(IIndex<TKey, T> serviceImplementations) {
            _serviceImplementations = serviceImplementations;
        }

        public T GetInstance(TKey key) => _serviceImplementations[key];
    }
}
