using System;
using System.Collections.Concurrent;
using CloudHeavenApi.Services;

namespace CloudHeavenApi.Implementation
{
    public class CacheManager<T> : ICacheService<T>
    {
        private readonly ConcurrentDictionary<string, T> _cache = new ConcurrentDictionary<string, T>();

        public void SetItem(string id, T item)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }
            _cache[id] = item;
        }

        public bool TryGetItem(string id, out T item)
        {
            if (string.IsNullOrEmpty(id))
            {
                item = default(T);
                return false;
            }
            return _cache.TryGetValue(id, out item);
        }

        public bool RemoveItem(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }
            return _cache.TryRemove(id, out _);
        }

        public bool TryUpdate(string id, Action<T> update)
        {
            if (!_cache.TryGetValue(id, out var item)) return false;
            update(item);
            return true;
        }
    }
}