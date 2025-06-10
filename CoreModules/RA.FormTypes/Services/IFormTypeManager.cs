using RA.Data.Domain.FormTypes;
using RA.Data.Domain.Settings;
using RA.FormTypes.Data;
using RA.FormTypes.Domain;

namespace RA.FormTypes.Services
{
    public interface IFormTypeManager
    {
        Task Delete(FormType formType);
        Task<FormType> GetById(Guid id);
        
        Task<IEnumerable<FormType>> GetList(string searchQuery = null, DateTime? createdOn = null, string formClassificationName = null);
        Task<FormType> GetTypeByEntityClassificationName(string systemName, string formTypeClassName);
        Task<FormType> GetTypeBySystemName(string systemName);
        Task<List<FormType>> GetTypesBySystemName(string systemName);
        Task Insert(FormType formType);
        Task Update(FormType formType);

        Task<Setting> GetSettingById(Guid id);
        Task<Setting> GetSettingByFormSystemName<TForm, TSettings>()
            where TForm : BaseForm
            where TSettings : BaseFormSetting;
        Task<TSettings> GetSettingDataOfForm<TForm, TSettings>()
            where TForm : BaseForm
            where TSettings : BaseFormSetting;
        Task InsertFormSetting<TForm, TSettings>()
            where TForm : BaseForm
            where TSettings : BaseFormSetting;
        Task UpdateSettingDataOfForm<TForm, TSettings>(TSettings settings)
            where TForm : BaseForm
            where TSettings : BaseFormSetting;
        Task IncreaseTemplateCountOfForm<TForm, TSettings>()
            where TForm : BaseForm
            where TSettings : BaseFormSetting;

        Task<string> GetCurrentTemplateOfForm<TForm, TSettings>()
            where TForm : BaseForm
            where TSettings : BaseFormSetting;
    }
}