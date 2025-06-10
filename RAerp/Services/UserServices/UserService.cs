using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using RA.Data.App_Data;
using RA.Data.Domain.Users;
using RAerp.Helpers.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RAerp.Services.UserServices
{
    /// <summary>
    /// User crud operations
    /// for now services will be in the main web
    /// will move this to another class library
    /// </summary>
    public class UserService : IUserService
    {
        private readonly RAerpContext _erpContext;

        public UserService(RAerpContext erpContext)
        {
            _erpContext = erpContext;
        }

        #region User CRUD
        public async Task<IEnumerable<User>> GetUserList(
            string searchQuery = null,
            DateTime? createdOn = null)
        {
             var query = _erpContext.User.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(c => 
                c.FirstName.Contains(searchQuery.Trim()) || 
                c.LastName.Contains(searchQuery.Trim()) ||
                c.Email.Contains(searchQuery.Trim()) ||
                c.Username.Contains(searchQuery.Trim()));

            if (createdOn.HasValue)
                query = query.Where(c => c.CreatedOn >= createdOn.Value);

            query = query.OrderBy(c => c.CreatedOn);
            
            return await query.ToListAsync();
        }
        public virtual async Task<User> GetById(Guid id)
        {
            return await _erpContext.User.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<User> GetUserByUsername(string userName)
        {
            return await _erpContext.User.FirstOrDefaultAsync(c => c.Username.ToLower() == userName.ToLower());
        }

        public virtual async Task Insert(User entity)
        {
            entity.CreatedOn = DateTime.Now;
            entity.LastActivityDate = DateTime.Now;
            await _erpContext.User.AddAsync(entity);
            await _erpContext.SaveChangesAsync();
        }

        public virtual async Task Update(User entity)
        {
            _erpContext.User.Update(entity);
            await _erpContext.SaveChangesAsync();
        }

        public virtual async Task Delete(User entity)
        {
            _erpContext.User.Remove(entity);
            await _erpContext.SaveChangesAsync();
        }

        #endregion
        
        #region User Role

        public async Task<UserRole> GetUserRoleById(Guid id)
        {
            return await _erpContext.UserRole.Where(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<UserRole>> GetUserRoleList(
            string searchQuery = null,
            DateTime? createdOn = null,
            bool includeDeleted = false)
        {
            var query = _erpContext.UserRole.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(c => c.Rolename.Contains(searchQuery.Trim()));

            if (createdOn.HasValue)
                query = query.Where(c => c.CreatedOn >= createdOn.Value);

            if (!includeDeleted)
                query = query.Where(c => !c.Deleted);

            query = query.OrderBy(c => c.Rolename);

            return await query.ToListAsync();
        }

        public async Task InsertRole(UserRole entity)
        {
            entity.CreatedOn = DateTime.Now;
            await _erpContext.UserRole.AddAsync(entity);
            await _erpContext.SaveChangesAsync();
        }
        public async Task UpdateRole(UserRole entity)
        {
            _erpContext.UserRole.Update(entity);
            await _erpContext.SaveChangesAsync();
        }

        #endregion

        #region User > UserRole mapping
        public async Task<List<UserUserRoleMapping>> GetUserMappingList()
        {
            return await _erpContext.UserUserRoleMapping.ToListAsync();
        }

        public async Task<UserUserRoleMapping> GetUserRoleMappingByUserId(Guid id)
        {
            return await _erpContext.UserUserRoleMapping.FirstOrDefaultAsync(c => c.UserId == id && c.UserRoleId != Guid.Empty);
        }
        public async Task<List<UserUserRoleMapping>> GetUsersByUserRoleId(Guid id)
        {
            return await _erpContext.UserUserRoleMapping.Where(c => c.UserRoleId == id).ToListAsync();
        }

        public async Task InsertMapping(Guid userId, Guid userRoleId)
        {
            var mapping = new UserUserRoleMapping()
            {
                UserId = userId,
                UserRoleId = userRoleId
            };
            await _erpContext.UserUserRoleMapping.AddAsync(mapping);
            await _erpContext.SaveChangesAsync();
        }

        public async Task UpdateMapping(UserUserRoleMapping mapping)
        {
            _erpContext.UserUserRoleMapping.Update(mapping);
            await _erpContext.SaveChangesAsync();
        }
        #endregion

    }
}
