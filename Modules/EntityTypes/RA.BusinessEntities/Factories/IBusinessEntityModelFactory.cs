using RA.BusinessEntities.Domain;
using RA.Core.Models.PluginModels.BusinessEntities;

namespace RA.BusinessEntities.Factories
{
    public interface IBusinessEntityModelFactory
    {
        Task<BusinessEntityConfigureModel> PrepareBusinessEntityConfigureModel(Guid entityTypeId, string systemNamee);
        Task<BusinessEntityModel> PrepareBusinessEntityModel(BusinessEntityModel BusinessEntityModel, BusinessEntity businessEntity, Guid entityTypeId);
        Task<BusinessEntitySearchModel> PrepareBusinessEntitySearchModel(BusinessEntitySearchModel searchModel, int pageSize, int pageNumber);
        Task<BusinessEntityListModel> PrepareBusinessEntityListModel(BusinessEntitySearchModel searchModel);
    }
}