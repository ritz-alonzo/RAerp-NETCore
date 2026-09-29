using RAerp.Domain.Settings;
using System;

namespace RAerp.Services.Configurations
{
    public interface ISettingService
    {
        Task DeleteSetting(Setting setting);
        Task<Setting> GetSettingById(Guid id);
        Task<Setting> GetSettingByName(string name);
        Task<Setting> GetSettingBySystemName(string systemName);
        Task InsertSetting(Setting setting);
        Task UpdateSetting(Setting setting);
    }
}