using RAerp.Security.Idempontency.Records;

namespace RAerp.Security.Idempontency.Services.RedisCacheService
{
    public interface IRedisCacheService
    {
        Task<IdempotentMetadata> GetResponseAsync(string key, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        Task SetResponseAsync(string key, IdempotentMetadata response, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    }
}