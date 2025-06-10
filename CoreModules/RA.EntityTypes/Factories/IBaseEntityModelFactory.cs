using RA.Core.Domain;
using RA.Core.Models.BaseModels;
using RA.Core.Models.PluginModels.EntityTypes;
using RA.Data.Domain.EntityTypes;
using RA.EntityTypes.Data;
using RA.EntityTypes.Domain;

namespace RA.EntityTypes.Factories
{
    public interface IBaseEntityModelFactory
    {
        TListModel PrepareBaseEntityListModel<TListModel, TModelList, TSearch>(TListModel list, List<TModelList> listModel, TSearch searchModel, int totalItems)
            where TListModel : BaseListModel<TModelList>
            where TModelList : BaseEntityModel
            where TSearch : BaseEntitySearchModel;
        TModel PrepareBaseEntityModel<TModel, TEntity, TSettings>(TModel model, TEntity entity, TSettings settings)
            where TModel : BaseEntityModel
            where TSettings : BaseEntityTypeSetting
            where TEntity : BaseEntityType;
        TSearch PrepareBaseEntitySearchModel<TSearch>(TSearch searchModel, EntityType entityType, int pageSize, int pageNumber)
            where TSearch : BaseEntitySearchModel;
        TModel PrepareBaseEntityConfigureModel<TModel, TEntity, TSettings>(TModel configureModel, Guid entityTypeId, string entityTypeSystemName = null)
            where TModel : BaseEntityConfigureModel
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting;
        TModel PrepareBaseEntityModelUIAccess<TModel, TEntity, TSettings>(TModel model, TEntity entityType, TSettings settings)
            where TModel : BaseEntityModel
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting;
        TList PrepareBaseEntityListModelUIAccess<TList, TModel, TEntity, TSettings>(TList model, Guid entityTypeId)
            where TList : BaseListModel<TModel>
            where TModel : BaseEntityModel
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting;
        TModel PrepareBaseEntityPortableView<TModel, TEntity>(TModel model, TEntity entity)
            where TModel : BaseEntityModel
            where TEntity : BaseEntityType;
    }
}