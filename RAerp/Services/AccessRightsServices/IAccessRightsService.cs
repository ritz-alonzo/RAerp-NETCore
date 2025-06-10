using RA.Data.Domain.AccessRightControl;

namespace RAerp.Services.AccessRightsServices
{
    public interface IAccessRightsService
    {
        Task Delete(AccessRights accessRights);
        Task<AccessRights> GetAccessRightsByUserRoleId(Guid userRoleId);
        Task<AccessRights> GetById(Guid id);
        Task<IEnumerable<AccessRights>> GetList();
        Task Insert(AccessRights accessRights);
        Task Update(AccessRights accessRights);
    }
}