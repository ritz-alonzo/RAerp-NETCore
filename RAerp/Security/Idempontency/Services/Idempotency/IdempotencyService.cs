using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RAerp.App_Data;
using RAerp.Security.Idempontency.Domain;
using RAerp.Security.Idempontency.Records;
using RAerp.Security.Idempontency.Services.RedisCacheService;
using RAerp.Security.Idempontency.Services.SqlLock;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RAerp.Security.Idempontency.Services.Idempotency
{
    public class IdempotencyService : IIdempotencyService
    {
        private readonly RAerpContext _db;
        private readonly IRedisCacheService _redis;
        private readonly ISqlLockService _sqlLock;
        private readonly ILogger<IdempotencyService> _logger;

        private IDbContextTransaction _transaction;

        public IdempotencyService(
            RAerpContext db,
            IRedisCacheService redis,
            ISqlLockService sqlLock,
            ILogger<IdempotencyService> logger)
        {
            _db = db;
            _redis = redis;
            _sqlLock = sqlLock;
            _logger = logger;
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
                return;

            _transaction =
                await _db.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
                return;

            await _transaction.CommitAsync(cancellationToken);

            await _transaction.DisposeAsync();

            _transaction = null;
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
                return;

            await _transaction.RollbackAsync(cancellationToken);

            await _transaction.DisposeAsync();

            _transaction = null;
        }

        public async Task AcquireLockAsync(string idempotencyKey, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Acquiring SQL Lock {Key}",
                idempotencyKey);

            await _sqlLock.AcquireAsync(
                _db,
                idempotencyKey,
                cancellationToken);
        }

        #region Cache Methods

        public async Task<IdempotentMetadata> GetCachedResponseAsync(string idempotencyKey, CancellationToken cancellationToken = default)
        {
            return await _redis.GetResponseAsync(
                idempotencyKey,
                cancellationToken);
        }

        public async Task CacheResponseAsync(IdempotencyRecord record, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(record.Response))
                return;

            var response = new IdempotentMetadata(
                record.Key,
                record.Status,
                record.PayloadHash,
                record.StatusCode,
                record.Response,
                record.CreatedUtc
            );

            await _redis.SetResponseAsync(
                record.Key,
                response,
                TimeSpan.FromMinutes(30),
                cancellationToken);
        }

        #endregion

        #region Database Methods

        public async Task<IdempotencyRecord> GetRecordAsync(string idempotencyKey, CancellationToken cancellationToken = default)
        {
            return await _db.IdempotencyRecord.AsNoTracking().FirstOrDefaultAsync(x => x.Key == idempotencyKey, cancellationToken);
        }

        public Task ValidateRequestHashAsync(IdempotencyRecord record, object request, CancellationToken cancellationToken = default)
        {
            var hash = ComputeStorageHash(request);

            if (!hash.Equals(record.PayloadHash, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "The Idempotency-Key has already been used with a different request.");
            }

            return Task.CompletedTask;
        }

        public async Task CreateProcessingRecordAsync(string idempotencyKey, string endpoint, object request, CancellationToken cancellationToken = default)
        {
            var record = new IdempotencyRecord
            {
                Id = Guid.NewGuid(),
                Key = idempotencyKey,
                Endpoint = endpoint,
                PayloadHash = ComputeStorageHash(request),
                Status = "Processing",
                CreatedUtc = DateTime.UtcNow
            };

            await _db.IdempotencyRecord.AddAsync(record);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task CompleteAsync(string idempotencyKey, ObjectResult result, CancellationToken cancellationToken = default)
        {
            var record = await GetRecordAsync(
                idempotencyKey,
                cancellationToken);

            if (record == null)
                throw new InvalidOperationException(
                    "Idempotency record not found.");

            record.Status = "Completed";
            record.StatusCode = result.StatusCode ?? 200;
            record.Response = JsonSerializer.Serialize(result.Value);
            record.CompletedUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            await CacheResponseAsync(record, cancellationToken);
        }

        public async Task CompleteAsync(string idempotencyKey, IActionResult result, CancellationToken cancellationToken = default)
        {
            switch (result)
            {
                case ObjectResult objectResult:
                    await CompleteAsync(
                        idempotencyKey,
                        objectResult,
                        cancellationToken);
                    return;

                case StatusCodeResult status:
                    {
                        var record = await GetRecordAsync(
                            idempotencyKey,
                            cancellationToken);

                        if (record == null)
                            throw new InvalidOperationException();

                        record.Status = "Completed";
                        record.StatusCode = status.StatusCode;
                        record.Response = string.Empty;
                        record.CompletedUtc = DateTime.UtcNow;

                        await _db.SaveChangesAsync(cancellationToken);

                        await CacheResponseAsync(record, cancellationToken);

                        return;
                    }

                default:
                    throw new NotSupportedException(
                        $"Unsupported IActionResult type: {result.GetType().Name}");
            }
        }

        public async Task MarkFailedAsync(string idempotencyKey, Exception exception, CancellationToken cancellationToken = default)
        {
            var record = await GetRecordAsync(
                idempotencyKey,
                cancellationToken);

            if (record == null)
                return;

            record.Status = "Failed";
            record.Error = exception.Message;
            record.CompletedUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogError(
                exception,
                "Idempotency failed. Key={Key}",
                idempotencyKey);
        }

        #endregion

        private string ComputeStorageHash(object request)
        {
            if (request == null) return string.Empty;

            var convertedRequest = JsonSerializer.Serialize(request);

            var bytes = Encoding.UTF8.GetBytes(convertedRequest);
            return Convert.ToBase64String(SHA256.HashData(bytes));
        }
    }
}
