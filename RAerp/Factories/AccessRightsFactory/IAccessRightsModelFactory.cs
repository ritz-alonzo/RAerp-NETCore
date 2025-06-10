using RAerp.Models.AccessRightsControlModel;

namespace RAerp.Factories.AccessRightsFactory
{
    public interface IAccessRightsModelFactory
    {
        Task<AccessRightsListModel> PrepareAccessRightsListModel(AccessRightsSearchModel searchModel);
        Task<AccessRightsSearchModel> PrepareAccessRightsSearchModel(AccessRightsSearchModel searchModel, int pageNumber, int pageSize);
    }
}