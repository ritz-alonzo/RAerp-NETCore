using RAerp.Models.ApplicationSettingsModel;

namespace RAerp.Factories.ApplicationSettingFactory
{
    public interface IApplicationSettingModelFactory
    {
        Task<ApplicationSettingModel> PrepareApplicationSettingModelAsync(ApplicationSettingModel model = null, Guid? id = null);
    }
}