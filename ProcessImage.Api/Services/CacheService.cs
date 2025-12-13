using Microsoft.Extensions.Caching.Memory;

using ProcessImage.Services.Interface;

namespace ProcessImage.Services
{

    public class CacheService : ICacheService
    {

        private readonly IMemoryCache _memoryCache;
        private readonly HashSet<string> _cacheKeys;

        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _cacheKeys = new HashSet<string>();
        }

        public void Set<T>(string key, T value, TimeSpan? expiration = null)
        {
            if(string.IsNullOrEmpty(key))
                throw new ArgumentException("Cache key nu poate fi gol", nameof(key));
            if(value == null)
                throw new ArgumentNullException(nameof(value), "Valoarea cache nu poate fi null");
            var cacheOptions = new MemoryCacheEntryOptions();

            if (expiration.HasValue)
                cacheOptions.AbsoluteExpirationRelativeToNow = expiration;
            else
                cacheOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            _memoryCache.Set(key, value, cacheOptions);
            _cacheKeys.Add(key);
        }
        public T Get<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Cache key nu poate fi gol", nameof(key));
            if (_memoryCache.TryGetValue(key, out T value))
                return value;
            return default;
        }
        public bool TryGet<T>(string key , out T value)
        {
            if(string.IsNullOrEmpty(key))
            {
                value = default;
                return false;
            }
            return _memoryCache.TryGetValue(key, out value);
        }

        public void Update<T>(string key, T value, TimeSpan? expiration = null)
        {
            if(string.IsNullOrEmpty(key))
                throw new ArgumentException("Cache key nu poate fi gol", nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            _memoryCache.Remove(key);
            Set(key, value,expiration);
        }

        public void Remove(string key)
        {
            if(string.IsNullOrEmpty(key))
                throw new ArgumentException("Cache key nu poate fi gol", nameof(key));
            _memoryCache.Remove(key);
            _cacheKeys.Remove(key);
        }
        public void RemoveByPattern(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentException("Pattern nu poate fi gol", nameof(pattern));

            var keysToRemove = new List<string>();
            foreach(var key in _cacheKeys)
            {
                if (key.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    keysToRemove.Add(key);
            }
            foreach (var key in keysToRemove)
                Remove(key);
        }

        public bool Exists(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            return _memoryCache.TryGetValue(key,out _);

        }
        public void Clear()
        {
            var keysToRemove = new List<string>(_cacheKeys);
            foreach (var key in keysToRemove)
                _memoryCache.Remove(key);
            _cacheKeys.Clear();
        }
    }
}
