using Microsoft.Extensions.Caching.Distributed;
using RAerp.Security.Idempontency.Records;
using System.Text.Json;

namespace RAerp.Security.Idempontency.Services.RedisCacheService
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        private static string BuildKey(string key)
            => $"idem:{key}";

        public async Task<IdempotentMetadata> GetResponseAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            var json = await _cache.GetStringAsync(
                BuildKey(key),
                cancellationToken);

            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<IdempotentMetadata>(json);
        }

        public async Task SetResponseAsync(
            string key,
            IdempotentMetadata response,
            TimeSpan? expiration = null,
            CancellationToken cancellationToken = default)
        {
            expiration ??= TimeSpan.FromMinutes(30);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            var json = JsonSerializer.Serialize(response);

            await _cache.SetStringAsync(
                BuildKey(key),
                json,
                options,
                cancellationToken);
        }

        public async Task RemoveAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            await _cache.RemoveAsync(
                BuildKey(key),
                cancellationToken);
        }
    }
}
