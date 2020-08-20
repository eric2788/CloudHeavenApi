using CloudHeavenApi.Services;
using System.Collections.Concurrent;

namespace CloudHeavenApi.Implementation
{
    public class CacheManager<T> : ICacheService<T>
    {
        private readonly ConcurrentDictionary<string, T> _cache = new ConcurrentDictionary<string, T>();
        public void SetItem(string id, T item)
        {
            _cache[id] = item;
        }

        public bool TryGetItem(string id, out T item)
        {
            return _cache.TryGetValue(id, out item);
        }

        public bool RemoveItem(string id)
        {
            return _cache.TryRemove(id, out _);
        }
    }
}
