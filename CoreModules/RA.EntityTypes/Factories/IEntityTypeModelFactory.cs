using RA.Core.Models.PluginModels.EntityTypes;

namespace RA.EntityTypes.Factories
{
    public interface IEntityTypeModelFactory
    {
        Task<EntityTypeModel> PrepareEntityTypeModelAsync(EntityTypeModel model, Guid entityTypeId, bool childEntityCreation = false);
        Task<EntityTypeSearchModel> PrepareEntityTypeSearchModelAsync(EntityTypeSearchModel searchModel, int pageNumber, int pageSize);
        Task<EntityTypeListModel> PrepareEntityTypeListModelAsync(EntityTypeSearchModel searchModel);
        Task<EntityTypeListModel> PrepareChildEntityTypeListModelAsync(EntityTypeSearchModel searchModel);
    }
}