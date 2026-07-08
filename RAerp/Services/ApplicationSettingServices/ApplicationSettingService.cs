using Microsoft.EntityFrameworkCore;
using RA.Data.App_Data;
using RA.Data.Domain.Application;

namespace RAerp.Services.ApplicationSettingServices
{
    public class ApplicationSettingService : IApplicationSettingService
    {
        private readonly RAerpContext _context;
        private readonly DbSet<ApplicationSetting> _applicationSettingRepository;

        public ApplicationSettingService(RAerpContext context)
        {
            _context = context;
            _applicationSettingRepository = _context.Set<ApplicationSetting>();
        }

        public async Task<ApplicationSetting> GetByIdAsync(Guid id)
        {
            return await _applicationSettingRepository
                .FirstOrDefaultAsync(x => x.Id == id && !x.Deleted);
        }

        public async Task<ApplicationSetting> GetCurrentApplicationSettingAsync()
        {
            return await _applicationSettingRepository.FirstOrDefaultAsync();
        }

        public async Task<IList<ApplicationSetting>> GetListAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _applicationSettingRepository
                .Where(x => !x.Deleted)
                .OrderByDescending(x => x.CreatedOn);

            if (pageSize != int.MaxValue)
            {
                query = (IOrderedQueryable<ApplicationSetting>)query
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize);
            }

            return await query.ToListAsync();
        }

        public async Task InsertAsync(ApplicationSetting applicationSetting)
        {
            if (applicationSetting == null)
                throw new ArgumentNullException(nameof(applicationSetting));

            applicationSetting.CreatedOn = DateTime.UtcNow;
            
            await _applicationSettingRepository.AddAsync(applicationSetting);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ApplicationSetting applicationSetting)
        {
            if (applicationSetting == null)
                throw new ArgumentNullException(nameof(applicationSetting));

            applicationSetting.ModifiedOn = DateTime.UtcNow;

            _applicationSettingRepository.Update(applicationSetting);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ApplicationSetting applicationSetting)
        {
            if (applicationSetting == null)
                throw new ArgumentNullException(nameof(applicationSetting));

            // Soft delete
            applicationSetting.Deleted = true;
            applicationSetting.DeletedOn = DateTime.UtcNow;

            await UpdateAsync(applicationSetting);
        }
    }
}