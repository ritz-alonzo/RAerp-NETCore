using Microsoft.AspNetCore.Mvc.ModelBinding;
using RA.Core.Domain;
using RA.Core.Models.BaseModels;

namespace RAerp.Factories.CoreFactories
{
    public interface IBaseAdminModelFactory
    {
        TSearch PrepareBaseAdminSearchModel<TSearch>(TSearch searchModel, int pageSize, int pageNumber)
            where TSearch : BaseSearchModel;
        TList PrepareBaseAdminListModel<TList, TModel>(TList list, List<TModel> listModel, int pageSize, int pageNumber, int skip, int totalItems)
            where TList : BaseAdminListModel<TModel>
            where TModel : BaseAdminModel;
        TModel PrepareBaseAdminModel<TModel, TEntity>(TModel model, TEntity entity)
            where TModel : BaseAdminModel
            where TEntity : BaseAdminEntity;
    }
}