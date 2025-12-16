using Microsoft.Extensions.Caching.Memory;

namespace LibraryManagementSystem.Components
{
    public class CachingService
    {
        private readonly IMemoryCache _cache;

        public CachingService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            if (!_cache.TryGetValue(key, out T value))
            {
                value = await factory();
                var options = new MemoryCacheEntryOptions();
                if (expiration.HasValue)
                    options.AbsoluteExpirationRelativeToNow = expiration;
                _cache.Set(key, value, options);
            }
            return value;
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }
    }
}