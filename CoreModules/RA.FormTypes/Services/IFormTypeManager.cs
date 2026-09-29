using RA.FormTypes.Data;
using RA.FormTypes.Domain;
using RAerp.Domain.Settings;

namespace RA.FormTypes.Services
{
    public interface IFormTypeManager
    {
        Task<Setting> GetSettingByIdAsync(Guid id);
        Task<Setting> GetSettingByFormSystemNameAsync<TForm, TSettings>()
            where TForm : BaseForm
            where TSettings : BaseFormSetting;
        Task<TSettings> GetSettingDataOfFormAsync<TForm, TSettings>()
            where TForm : BaseForm
            where TSettings : BaseFormSetting;
        Task InsertFormSettingAsync<TForm, TSettings>()
            where TForm : BaseForm
            where TSettings : BaseFormSetting;
        Task UpdateSettingDataOfFormAsync<TForm, TSettings>(TSettings settings)
            where TForm : BaseForm
            where TSettings : BaseFormSetting;
        Task IncreaseTemplateCountOfFormAsync<TForm, TSettings>()
            where TForm : BaseForm
            where TSettings : BaseFormSetting;

        Task<string> GetCurrentTemplateOfFormAsync<TForm, TSettings>()
            where TForm : BaseForm
            where TSettings : BaseFormSetting;
    }
}