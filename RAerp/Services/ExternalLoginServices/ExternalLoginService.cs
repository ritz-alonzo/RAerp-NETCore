using Microsoft.EntityFrameworkCore;
using RA.Core.DataCaching.CacheManagement;
using RA.WebFramework.Extensions;
using RA.WebFramework.Models.Pagination;
using RAerp.App_Data;
using RAerp.Domain.Application;
using RAerp.Domain.Users;
using RAerp.Services.ApplicationSettingServices;

namespace RAerp.Services.ExternalLoginServices
{
    public class ExternalLoginService : IExternalLoginService
    {
        #region Constants
        private readonly RAerpContext _context;
        private readonly ICacheManager<ExternalLogin> _externalLoginCacheManager;
        private readonly IApplicationSettingService _applicationSettingService;
        private readonly ApplicationSetting _appSettings;

        public ExternalLoginService(RAerpContext context,
            ICacheManager<ExternalLogin> externalLoginCacheManager,
            IApplicationSettingService applicationSettingService)
        {
            _context = context;
            _externalLoginCacheManager = externalLoginCacheManager;
            _applicationSettingService = applicationSettingService;
            _appSettings = _applicationSettingService.GetCurrentApplicationSettingAsync().Result; ;
        }
        #endregion

        #region External Login
        public async Task<ExternalLogin> GetByIdAsync(Guid id)
        {
            return await _context.ExternalLogin.FirstOrDefaultAsync(c => c.Id == id);
        }
        public async Task<ExternalLogin> GetByUserIdAsync(Guid userId)
        {
            return await _context.ExternalLogin.FirstOrDefaultAsync(c => c.UserId == userId);
        }
        public async Task<ExternalLogin> GetByProviderAndUserIdAsync(string provider, Guid userId)
        {
            return await _context.ExternalLogin.FirstOrDefaultAsync(c => c.Provider == provider && c.UserId == userId);
        }
        public async Task<ExternalLogin> GetByProviderAndSubjectIdAsync(string provider,string subjectId)
        {
            return await _context.ExternalLogin.FirstOrDefaultAsync(c => c.Provider == provider && c.ProviderSubjectId == subjectId);
        }
        public async Task<ExternalLogin> GetByProviderAndProviderEmailAsync(string provider, string email)
        {
            return await _context.ExternalLogin.FirstOrDefaultAsync(c => c.Provider == provider && c.ProviderEmail == email);
        }
        public async Task<ExternalLogin> GetByProviderNameAsync(string providerName)
        {
            return await _context.ExternalLogin.FirstOrDefaultAsync(c => c.ProviderName == providerName);
        }

        public async Task<IEnumerable<ExternalLogin>> GetListAsync(Guid? userId = null,
            string provider = null,
            string providerSubjectId = null,
            string providerEmail = null,
            string providerName = null,
            DateTime? createdOn = null,
            DateTime? lastLoginOn = null)
        {
            var query = _context.ExternalLogin.AsNoTracking();

            if (userId.IsNotNullOrEmpty())
                query = query.Where(c => c.UserId == userId.Value);

            if (!string.IsNullOrEmpty(provider))
                query = query.Where(c => c.Provider == provider);

            if (!string.IsNullOrEmpty(providerSubjectId))
                query = query.Where(c => c.ProviderSubjectId == providerSubjectId);

            if (!string.IsNullOrEmpty(providerEmail))
                query = query.Where(c => c.ProviderEmail == providerEmail);

            if (!string.IsNullOrEmpty(providerName))
                query = query.Where(c => c.ProviderName == providerName);

            if (createdOn.HasValue)
                query = query.Where(c => c.CreatedOn >= createdOn);

            if (lastLoginOn.HasValue)
                query = query.Where(c => c.LastLoginOn >= lastLoginOn);

            return await query.ToListAsync();
        }

        public async Task<PagedResult<ExternalLogin>> GetPaagedResultListAsync(Guid? userId = null,
            string provider = null,
            string providerSubjectId = null,
            string providerEmail = null,
            string providerName = null,
            DateTime? createdOn = null,
            DateTime? lastLoginOn = null,
            int? pageNumber = 0,
            int? pageSize = int.MaxValue)
        {
            var query = _context.ExternalLogin.AsNoTracking();

            if (userId.IsNotNullOrEmpty())
                query = query.Where(c => c.UserId == userId.Value);

            if (!string.IsNullOrEmpty(provider))
                query = query.Where(c => c.Provider == provider);

            if (!string.IsNullOrEmpty(providerSubjectId))
                query = query.Where(c => c.ProviderSubjectId == providerSubjectId);

            if (!string.IsNullOrEmpty(providerEmail))
                query = query.Where(c => c.ProviderEmail == providerEmail);

            if (!string.IsNullOrEmpty(providerName))
                query = query.Where(c => c.ProviderName == providerName);

            if (createdOn.HasValue)
                query = query.Where(c => c.CreatedOn >= createdOn);

            if (lastLoginOn.HasValue)
                query = query.Where(c => c.LastLoginOn >= lastLoginOn);

            return query.ToPagedResult(pageNumber, pageSize);
        }

        public async Task InsertAsync(ExternalLogin externalLogin)
        {
            if (externalLogin == null)
                throw new ArgumentNullException(nameof(externalLogin));

            externalLogin.Id = Guid.NewGuid();
            externalLogin.CreatedOn = DateTime.UtcNow;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.ExternalLogin.AddAsync(externalLogin);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

            }
            catch
            {
                await transaction.RollbackAsync();
                throw new Exception("Failed to insert external login.");
            }
        }

        public async Task UpdateAsync(ExternalLogin externalLogin)
        {
            if (externalLogin == null)
                throw new ArgumentNullException(nameof(externalLogin));

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ExternalLogin.Update(externalLogin);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw new Exception("Failed to update external login.");
            }
        }

        public async Task DeleteAsync(ExternalLogin externalLogin)
        {
            if (externalLogin == null)
                throw new ArgumentNullException(nameof(externalLogin));

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ExternalLogin.Remove(externalLogin);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw new Exception("Failed to delete external login.");
            }
        }
        #endregion
    }
}
