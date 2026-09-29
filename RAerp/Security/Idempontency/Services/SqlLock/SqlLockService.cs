using Microsoft.EntityFrameworkCore;

namespace RAerp.Security.Idempontency.Services.SqlLock
{
    public class SqlLockService : ISqlLockService
    {
        public async Task AcquireAsync(
            DbContext context,
            string key,
            CancellationToken cancellationToken)
        {
            await context.Database.ExecuteSqlRawAsync(
                """
                EXEC sp_getapplock
                     @Resource = {0},
                     @LockMode = 'Exclusive',
                     @LockOwner = 'Transaction',
                     @LockTimeout = 30000;
                """,
                new object[] { $"idem:{key}" },
                cancellationToken);
        }

        public async Task ReleaseAsync(
            DbContext context,
            string key,
            CancellationToken cancellationToken)
        {
            await context.Database.ExecuteSqlRawAsync(
                """
                EXEC sp_releaseapplock
                     @Resource = {0},
                     @LockOwner = 'Transaction';
                """,
                new object[] { $"idem:{key}" },
                cancellationToken);
        }
    }
}
