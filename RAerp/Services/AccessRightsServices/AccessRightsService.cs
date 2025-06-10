using RA.Data.App_Data;
using RA.Data.Domain.AccessRightControl;
using Microsoft.EntityFrameworkCore;

namespace RAerp.Services.AccessRightsServices
{
    public class AccessRightsService : IAccessRightsService
    {
        private readonly RAerpContext _erpContext;

        public AccessRightsService(RAerpContext erpContext)
        {
            _erpContext = erpContext;
        }

        #region CRUD
        public async Task<IEnumerable<AccessRights>> GetList()
        {
            return await _erpContext.AccessRights.ToListAsync();
        }

        public async Task<AccessRights> GetById(Guid id)
        {
            return await _erpContext.AccessRights.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<AccessRights> GetAccessRightsByUserRoleId(Guid userRoleId)
        {
            return await _erpContext.AccessRights.FirstOrDefaultAsync(c => c.UserRoleId == userRoleId);
        }

        public async Task Insert(AccessRights accessRights)
        {
            accessRights.CreatedOn = DateTime.Now;
            await _erpContext.AccessRights.AddAsync(accessRights);
            await _erpContext.SaveChangesAsync();
        }

        public async Task Update(AccessRights accessRights)
        {
            accessRights.ModifiedOn = DateTime.Now;
            _erpContext.AccessRights.Update(accessRights);
            await _erpContext.SaveChangesAsync();
        }

        public async Task Delete(AccessRights accessRights)
        {
            accessRights.DeletedOn = DateTime.Now;
            accessRights.Deleted = true;
            _erpContext.AccessRights.Update(accessRights);
            await _erpContext.SaveChangesAsync();
        }

        #endregion
    }
}
