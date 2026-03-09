using RA.Core.Models.BaseModels;
using RA.Core.Models.PluginModels.FormTypes;
using RA.FormTypes.Data;
using RA.FormTypes.Domain;

namespace RA.FormTypes.Factories
{
    public interface IBaseFormModelFactory
    {
        Task<TConfig> PrepareBaseFormConfigureModelAsync<TConfig, TForm, TSettings>(TConfig configureModel)
            where TConfig : BaseFormConfigureModel
            where TForm : BaseForm
            where TSettings : BaseFormSetting;
        Task<TList> PrepareBaseFormListModelAsync<TList, TModel, TSearch, TForm, TSettings>(TList list, List<TModel> listModel, TSearch searchModel, int totalItems)
            where TList : BaseListModel<TModel>
            where TModel : BaseFormModel
            where TSearch : BaseFormSearchModel
            where TForm : BaseForm
            where TSettings : BaseFormSetting;
        Task<TList> PrepareBaseFormListModelUIAccessAsync<TList, TModel, TForm, TSettings>(TList model)
            where TList : BaseListModel<TModel>
            where TModel : BaseFormModel
            where TForm : BaseForm
            where TSettings : BaseFormSetting;
        Task<TModel> PrepareBaseFormModelAsync<TModel, TForm, TSettings>(TModel model, TForm form, TSettings settings)
            where TModel : BaseFormModel
            where TForm : BaseForm
            where TSettings : BaseFormSetting;
        Task<TModel> PrepareBaseFormModelUIAccessAsync<TModel, TForm, TSettings>(TModel model, TForm form, TSettings settings)
            where TModel : BaseFormModel
            where TForm : BaseForm
            where TSettings : BaseFormSetting;
        TSearch PrepareBaseFormSearchModel<TSearch, TForm>(TSearch searchModel, int pageSize, int pageNumber)
            where TSearch : BaseFormSearchModel
            where TForm : BaseForm;
        TModel PrepareBaseFormViewComponent<TModel, TForm>(TModel model, TForm entity)
            where TModel : BaseFormModel
            where TForm : BaseForm;
        TItemListModel PrepareBaseFormItemListModel<TItemListModel, TItemModel, TSettings>(TItemListModel listModel, List<TItemModel> formItemModelList, TSettings settings, int pageNumber, int totalItems)
            where TItemListModel : BaseFormItemListModel<TItemModel>
            where TItemModel : BaseFormItemModel
            where TSettings : BaseFormSetting;
    }
}