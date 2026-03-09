using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RA.Data.App_Data;
using RA.Data.Domain.Settings;
using RA.FormTypes.Data;
using RA.FormTypes.Domain;
using RA.WebFramework.Extensions;

namespace RA.FormTypes.Services
{
    public class FormTypeManager : IFormTypeManager
    {
        #region Constants
        private readonly RAerpContext _erpContext;
        #endregion

        #region Ctor
        public FormTypeManager(RAerpContext erpContext)
        {
            _erpContext = erpContext;
        }
        #endregion

        #region Settings

        public async Task<Setting> GetSettingByIdAsync(Guid id)
        {
            return await _erpContext.Setting.FirstOrDefaultAsync(c => c.Id == id);
        }

        public virtual async Task<Setting> GetSettingByFormSystemNameAsync<TForm, TSettings>()
            where TForm : BaseForm
            where TSettings : BaseFormSetting
        {
            return await _erpContext.Setting.FirstOrDefaultAsync(c => c.SystemName.Equals(typeof(TForm).FullName));
        }

        /// <summary>
        /// Insert setting will insert TSettings 
        /// already converted from model to Setting of Form
        /// Data of Setting will be converted here
        /// </summary>
        /// <param name="settings"></param>
        public virtual async Task InsertFormSettingAsync<TForm, TSettings>()
            where TForm : BaseForm
            where TSettings : BaseFormSetting
        {
            var setting = new Setting()
            {
                Id = Guid.NewGuid(),
                Name = typeof(TForm).Name,
                SystemName = typeof(TForm).FullName,
                Data = "{}",
                CreatedOn = DateTime.Now,
            };
            await _erpContext.Setting.AddAsync(setting);
            await _erpContext.SaveChangesAsync();
        }

        /// <summary>
        /// Insert setting will insert TSettings 
        /// already converted from model to Setting of Form
        /// Data of Setting will be converted here
        /// </summary>
        /// <param name="settings"></param>
        public virtual async Task UpdateSettingDataOfFormAsync<TForm, TSettings>(TSettings settings)
            where TForm : BaseForm
            where TSettings : BaseFormSetting
        {
            var settingData = JsonConvert.SerializeObject(settings);

            var setting = await GetSettingByFormSystemNameAsync<TForm, TSettings>();
            if (setting != null)
            {
                setting.Data = settingData;
                setting.ModifiedOn = DateTime.Now;
            }
            else
            {
                return;
            }

            _erpContext.Setting.Update(setting);
            await _erpContext.SaveChangesAsync();
        }

        /// <summary>
        /// This method will be used to get saved setting of Form
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual async Task<TSettings> GetSettingDataOfFormAsync<TForm, TSettings>()
            where TForm : BaseForm
            where TSettings : BaseFormSetting
        {
            Setting setting = null;

            setting = await GetSettingByFormSystemNameAsync<TForm, TSettings>();
            if (setting == null)
            {
                await InsertFormSettingAsync<TForm, TSettings>();
                setting = await GetSettingByFormSystemNameAsync<TForm, TSettings>();
            }

            TSettings settingsData = new BaseFormSetting() as TSettings;
            if (setting.Data != null && setting.Data != "{}")
                settingsData = JsonConvert.DeserializeObject<TSettings>(setting.Data);

            return settingsData;

        }

        // will be used in insert, update of Form
        public virtual async Task<string> GetCurrentTemplateOfFormAsync<TForm, TSettings>()
            where TForm : BaseForm
            where TSettings : BaseFormSetting
        {
            var setting = await GetSettingDataOfFormAsync<TForm, TSettings>();

            if (setting == null)
                throw new Exception(nameof(setting));

            if (string.IsNullOrEmpty(setting.FormNbrTemplate))
                return null;

            return (setting.FormNbrCount + setting.FormNbrIncrementCount).ToString(setting.FormNbrTemplate);
        }

        // will be used in insert, update of Form
        public virtual async Task IncreaseTemplateCountOfFormAsync<TForm, TSettings>()
            where TForm : BaseForm
            where TSettings : BaseFormSetting
        {
            var setting = await GetSettingDataOfFormAsync<TForm, TSettings>();

            if (setting == null)
                throw new Exception(nameof(setting));

            setting.FormNbrCount += setting.FormNbrIncrementCount;

            await UpdateSettingDataOfFormAsync<TForm, TSettings>(setting);
        }

        #endregion
    }
}
