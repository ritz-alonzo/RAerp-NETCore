using Microsoft.EntityFrameworkCore;

namespace RAerp.Security.Idempontency.Services.SqlLock
{
    public interface ISqlLockService
    {
        Task AcquireAsync(DbContext context, string key, CancellationToken cancellationToken);
        Task ReleaseAsync(DbContext context, string key, CancellationToken cancellationToken);
    }
}