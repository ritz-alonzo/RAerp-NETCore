using RA.Core.Models.PluginModels.FormTypes;

namespace RA.FormTypes.Factories
{
    public interface IFormTypeModelFactory
    {
        FormTypeListModel PrepareFormTypeListModel(FormTypeSearchModel searchModel);
        FormTypeSearchModel PrepareFormTypeSearchModel(FormTypeSearchModel searchModel, int pageNumber, int pageSize);
    }
}