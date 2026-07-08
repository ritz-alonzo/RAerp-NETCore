using RA.Data.App_Data;
using RA.Data.Domain.Settings;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace RAerp.Services.Configurations
{
    public class SettingService : ISettingService
    {
        private readonly RAerpContext _erpContext;

        public SettingService(RAerpContext mmsContext)
        {
            _erpContext = mmsContext;
        }

        public async Task<Setting> GetSettingById(Guid id)
        {
            return await _erpContext.Setting.FirstAsync(c => c.Id == id);
        }

        public async Task<Setting> GetSettingByName(string name)
        {
            return await _erpContext.Setting.FirstAsync(c => c.Name.Equals(nameof(name), StringComparison.InvariantCultureIgnoreCase));
        }

        public async Task<Setting> GetSettingBySystemName(string systemName)
        {
            return await _erpContext.Setting.FirstAsync(c => c.SystemName.Equals(systemName, StringComparison.InvariantCultureIgnoreCase));
        }

        public async Task InsertSetting(Setting setting)
        {
            setting.CreatedOn = DateTime.UtcNow;
            await _erpContext.Setting.AddAsync(setting);
            await _erpContext.SaveChangesAsync();
        }

        public async Task UpdateSetting(Setting setting)
        {
            setting.ModifiedOn = DateTime.UtcNow;
            _erpContext.Setting.Update(setting);
            await _erpContext.SaveChangesAsync();
        }

        public async Task DeleteSetting(Setting setting)
        {
            _erpContext.Setting.Remove(setting);
            await _erpContext.SaveChangesAsync();
        }
    }
}
