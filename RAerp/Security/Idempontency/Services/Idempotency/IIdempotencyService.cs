using Microsoft.AspNetCore.Mvc;
using RAerp.Security.Idempontency.Domain;
using RAerp.Security.Idempontency.Records;

namespace RAerp.Security.Idempontency.Services.Idempotency
{
    public interface IIdempotencyService
    {
        Task AcquireLockAsync(string idempotencyKey, CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CacheResponseAsync(IdempotencyRecord record, CancellationToken cancellationToken = default);
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task CompleteAsync(string idempotencyKey, IActionResult result, CancellationToken cancellationToken = default);
        Task CompleteAsync(string idempotencyKey, ObjectResult result, CancellationToken cancellationToken = default);
        Task CreateProcessingRecordAsync(string idempotencyKey, string endpoint, object request, CancellationToken cancellationToken = default);
        Task<IdempotentMetadata> GetCachedResponseAsync(string idempotencyKey, CancellationToken cancellationToken = default);
        Task<IdempotencyRecord> GetRecordAsync(string idempotencyKey, CancellationToken cancellationToken = default);
        Task MarkFailedAsync(string idempotencyKey, Exception exception, CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
        Task ValidateRequestHashAsync(IdempotencyRecord record, object request, CancellationToken cancellationToken = default);
    }
}