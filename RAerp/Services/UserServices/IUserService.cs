using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Data.Domain.Users;
using System;
using System.Collections.Generic;

namespace RAerp.Services.UserServices
{
    public interface IUserService
    {
        #region User
        Task<IEnumerable<User>> GetUserList(
            string searchQuery = null,
            DateTime? createdOn = null);
        Task<User> GetById(Guid id);
        Task<User> GetUserByUsername(string userName);
        Task Insert(User entity);
        Task Update(User entity);
        Task Delete(User entity);
        #endregion

        #region User Role
        Task<IEnumerable<UserRole>> GetUserRoleList(
            string searchQuery = null,
            DateTime? createdOn = null,
            bool includeDeleted = false);
        Task<UserRole> GetUserRoleById(Guid id); 
        Task<UserRole> GetUserRoleByUserId(Guid id);
        Task<UserRole> GetUserRoleByRoleNameAsync(string roleName);
        Task InsertRole(UserRole entity);
        Task UpdateRole(UserRole entity);

        #endregion

        #region User Role Mapping
        Task<List<UserUserRoleMapping>> GetUserMappingList();
        Task<UserUserRoleMapping> GetUserRoleMappingByUserId(Guid id);
        Task<List<UserUserRoleMapping>> GetUsersByUserRoleId(Guid id);
        Task InsertMapping(Guid userId, Guid userRoleId);
        Task UpdateMapping(UserUserRoleMapping mapping);

        #endregion

        #region User SelectList
        Task<List<SelectListItem>> GetAvailableUsers();
        #endregion
    }
}