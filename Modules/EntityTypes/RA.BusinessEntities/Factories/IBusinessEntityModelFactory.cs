using RA.BusinessEntities.Domain;
using RA.Core.Models.PluginModels.BusinessEntities;

namespace RA.BusinessEntities.Factories
{
    public interface IBusinessEntityModelFactory
    {
        Task<BusinessEntityConfigureModel> PrepareBusinessEntityConfigureModelAsync(Guid entityTypeId, string systemNamee);
        Task<BusinessEntityModel> PrepareBusinessEntityModelAsync(BusinessEntityModel BusinessEntityModel, BusinessEntity businessEntity, Guid entityTypeId);
        Task<BusinessEntitySearchModel> PrepareBusinessEntitySearchModelAsync(BusinessEntitySearchModel searchModel, int pageSize, int pageNumber);
        Task<BusinessEntityListModel> PrepareBusinessEntityListModelAsync(BusinessEntitySearchModel searchModel);
    }
}