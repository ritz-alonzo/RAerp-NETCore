using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RA.Data.App_Data;
using RA.Data.Domain.FormTypes;
using RA.Data.Domain.Settings;
using RA.FormTypes.Data;
using RA.FormTypes.Domain;
using RA.WebFramework.Extensions;

namespace RA.FormTypes.Services
{
    public class FormTypeManager : IFormTypeManager
    {
        private readonly RAerpContext _erpContext;

        public FormTypeManager(RAerpContext erpContext)
        {
            _erpContext = erpContext;
        }

        #region CRUD

        public virtual async Task<FormType> GetById(Guid id)
        {
            return await _erpContext.FormType.FirstOrDefaultAsync(c => c.Id == id);
        }

        public virtual async Task<FormType> GetTypeBySystemName(string systemName)
        {
            return await _erpContext.FormType.FirstOrDefaultAsync(c => c.FormTypeSystemName.ToLower() == systemName.ToLower());
        }

        public virtual async Task<List<FormType>> GetTypesBySystemName(string systemName)
        {
            return await _erpContext.FormType.Where(c => c.FormTypeSystemName.Contains(systemName)).ToListAsync();
        }

        public virtual async Task<FormType> GetTypeByEntityClassificationName(string systemName, string formTypeClassName)
        {
            return await _erpContext.FormType.FirstOrDefaultAsync(c => c.FormTypeSystemName.ToLower() == systemName.ToLower() && c.FormTypeClassificationName.ToLower() == formTypeClassName.ToLower());
        }

        public virtual async Task<IEnumerable<FormType>> GetList(
            string searchQuery = null,
            DateTime? createdOn = null,
            string formClassificationName = null)
        {
            var query = _erpContext.FormType.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(c =>
                c.FormTypeName.ToLower().Equals(searchQuery.ToLower()) ||
                c.FormTypeSystemName.ToLower().Equals(searchQuery.ToLower()));

            if (!string.IsNullOrEmpty(formClassificationName))
                query = query.Where(c => c.FormTypeClassificationName.Contains(formClassificationName, StringComparison.InvariantCultureIgnoreCase));

            if (createdOn.HasValue)
                query = query.Where(c => c.InstalledOn >= createdOn.Value);

            query = query.OrderBy(c => c.InstalledOn);

            return await query.ToListAsync();
        }

        public virtual async Task Insert(FormType formType)
        {
            formType.InstalledOn = DateTime.Now;
            await _erpContext.FormType.AddAsync(formType);
            await _erpContext.SaveChangesAsync();
        }

        public virtual async Task Update(FormType formType)
        {
            _erpContext.FormType.Update(formType);
            await _erpContext.SaveChangesAsync();
        }

        public virtual async Task Delete(FormType formType)
        {
            formType.Installed = false;
            formType.UnInstalledOn = DateTime.Now;
            _erpContext.FormType.Update(formType);
            await _erpContext.SaveChangesAsync();
        }

        #endregion

        #region Settings

        public async Task<Setting> GetSettingById(Guid id)
        {
            return await _erpContext.Setting.FirstOrDefaultAsync(c => c.Id == id);
        }

        public virtual async Task<Setting> GetSettingByFormSystemName<TForm, TSettings>()
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
        public virtual async Task InsertFormSetting<TForm, TSettings>()
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
        public virtual async Task UpdateSettingDataOfForm<TForm, TSettings>(TSettings settings)
            where TForm : BaseForm
            where TSettings : BaseFormSetting
        {
            var settingData = JsonConvert.SerializeObject(settings);

            var setting = await GetSettingByFormSystemName<TForm, TSettings>();
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
        public virtual async Task<TSettings> GetSettingDataOfForm<TForm, TSettings>()
            where TForm : BaseForm
            where TSettings : BaseFormSetting
        {
            Setting setting = null;

            setting = await GetSettingByFormSystemName<TForm, TSettings>();

            if (setting == null)
            {
                await InsertFormSetting<TForm, TSettings>();
                setting = await GetSettingByFormSystemName<TForm, TSettings>();
            }

            TSettings settingsData = new BaseFormSetting() as TSettings;
            if (setting.Data != null && setting.Data != "{}")
                settingsData = JsonConvert.DeserializeObject<TSettings>(setting.Data);

            return settingsData;

        }

        // will be used in insert, update of Form
        public virtual async Task<string> GetCurrentTemplateOfForm<TForm, TSettings>()
            where TForm : BaseForm
            where TSettings : BaseFormSetting
        {
            var setting = await GetSettingDataOfForm<TForm, TSettings>();

            if (setting == null)
                throw new Exception(nameof(setting));

            if (string.IsNullOrEmpty(setting.Template))
                return null;

            return (setting.TemplateCount + setting.TemplateIncrementCount).ToString(setting.Template);
        }

        // will be used in insert, update of Form
        public virtual async Task IncreaseTemplateCountOfForm<TForm, TSettings>()
            where TForm : BaseForm
            where TSettings : BaseFormSetting
        {
            var setting = await GetSettingDataOfForm<TForm, TSettings>();

            if (setting == null)
                throw new Exception(nameof(setting));

            setting.TemplateCount += setting.TemplateIncrementCount;

            await UpdateSettingDataOfForm<TForm, TSettings>(setting);
        }

        #endregion
    }
}
