using RAerp.Domain.Application;

namespace RAerp.Services.ApplicationSettingServices
{
    public interface IApplicationSettingService
    {
        Task<ApplicationSetting> GetByIdAsync(Guid id);
        Task<ApplicationSetting> GetCurrentApplicationSettingAsync();
        Task<IList<ApplicationSetting>> GetListAsync(int pageIndex = 0, int pageSize = int.MaxValue);
        Task InsertAsync(ApplicationSetting applicationSetting);
        Task UpdateAsync(ApplicationSetting applicationSetting);
        Task DeleteAsync(ApplicationSetting applicationSetting);
    }
}