using RA.Core.Models.BaseModels;

namespace RAerp.Factories.CoreFactories
{
    public interface IBaseModelFactory
    {
        TSearch PrepareBaseSearchModel<TSearch>(TSearch searchModel, int pageSize, int pageNumber) 
            where TSearch : BaseSearchModel;
        TModel PrepareBaseModel<TModel>(TModel model)
            where TModel : BaseModel;
        TList PrepareBaseListModel<TList, TModel, TSearch>(TList list, List<TModel> listModel, TSearch searchModel, int totalItems)
            where TList : BaseListModel<TModel>
            where TModel : BaseModel
            where TSearch : BaseSearchModel;
        TList PrepareBaseListFormSettingsModel<TList, TModel>(TList listModel)
            where TList : BaseListModel<TModel>
            where TModel: BaseModel;
    }
}