using RA.Core.Models.PluginModels.EntityTypes;

namespace RA.EntityTypes.Factories
{
    public interface IEntityTypeModelFactory
    {
        Task<EntityTypeModel> PrepareEntityTypeModel(EntityTypeModel model, Guid entityTypeId, bool childEntityCreation = false);
        Task<EntityTypeSearchModel> PrepareEntityTypeSearchModel(EntityTypeSearchModel searchModel, int pageNumber, int pageSize);
        Task<EntityTypeListModel> PrepareEntityTypeListModel(EntityTypeSearchModel searchModel);
        Task<EntityTypeListModel> PrepareChildEntityTypeListModel(EntityTypeSearchModel searchModel);
    }
}