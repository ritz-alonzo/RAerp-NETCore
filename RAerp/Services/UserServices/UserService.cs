using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RA.Data.Data;
using RA.WebFramework.Extensions;
using RAerp.App_Data;
using RAerp.Domain.Users;
using RAerp.Helpers.Constants;
using RAerp.Helpers.Security;
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
             var query = _erpContext.User.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(c => 
                c.FirstName.Contains(searchQuery.Trim()) || 
                c.LastName.Contains(searchQuery.Trim()) ||
                EncryptionHelper.DecryptData(c.Email, c.Salt).Result.Contains(searchQuery) ||
                c.Username.Contains(searchQuery.Trim()));

            if (createdOn.HasValue)
                query = query.Where(c => c.CreatedOn.ConvertToUTC() >= createdOn.Value.ConvertToUTC());

            query = query.Where(c => c.AccountStatusId == (int)UserAccountStatus.Active);

            query = query.OrderBy(c => c.CreatedOn);
            
            return query.ToList();
        }
        public virtual async Task<User> GetById(Guid id)
        {
            return await _erpContext.User.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<User> GetUserByUsername(string userName)
        {
            return await _erpContext.User.FirstOrDefaultAsync(c => c.Username.ToLower() == userName.ToLower());
        }
        public async Task<User> GetUserByFirstNameAndLastNameAsync(string firstName, string lastName)
        {
            return await _erpContext.User.FirstOrDefaultAsync(c => c.FirstName.ToLower() == firstName.ToLower() && c.LastName.ToLower() == lastName.ToLower());
        }
        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _erpContext.User.FirstOrDefaultAsync(c => EncryptionHelper.DecryptData(c.Email, c.Salt).Result == email);
        }

        public virtual async Task Insert(User entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedOn = DateTime.UtcNow;
            entity.LastActivityDate = DateTime.UtcNow;
            await _erpContext.User.AddAsync(entity);
            await _erpContext.SaveChangesAsync();
        }

        public virtual async Task Update(User entity)
        {
            entity.ModifiedOn = DateTime.UtcNow;
            _erpContext.User.Update(entity);
            await _erpContext.SaveChangesAsync();
        }

        public virtual async Task Delete(User entity)
        {
            entity.Deleted = true;
            entity.DeletedOn = DateTime.UtcNow;
            _erpContext.User.Update(entity);
            await _erpContext.SaveChangesAsync();
        }

        public virtual async Task<bool> CheckUserIfExists(string username, string email = null)
        {
            bool isExists = false;
            var query = await _erpContext.User.AsNoTracking().AsQueryable().ToListAsync();

            if (!string.IsNullOrEmpty(username))
                isExists = query.Any(c => c.Username == username);
            //if (!string.IsNullOrEmpty(email) && !isExists)
            //    isExists = query.Any(c => EncryptionHelper.DecryptData(c.Email, c.Salt).Result == email);

            return isExists;
        }

        #endregion
        
        #region User Role

        public async Task<UserRole> GetUserRoleById(Guid id)
        {
            return await _erpContext.UserRole.Where(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task<UserRole> GetUserRoleByRoleNameAsync(string rolename)
        {
            return await _erpContext.UserRole.FirstOrDefaultAsync(c => c.Rolename == rolename);
        }

        public async Task<IEnumerable<UserRole>> GetUserRoleList(
            string searchQuery = null,
            DateTime? createdOn = null,
            bool includeDeleted = false)
        {
            var query = _erpContext.UserRole.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(c => c.Rolename.Contains(searchQuery.Trim()));

            if (createdOn.HasValue)
                query = query.Where(c => c.CreatedOn.ConvertToUTC() >= createdOn.Value.ConvertToUTC());

            if (!includeDeleted)
                query = query.Where(c => !c.Deleted);

            query = query.OrderBy(c => c.Rolename);

            return query.ToList();
        }

        public async Task<UserRole> GetUserRoleByUserId(Guid id)
        {
            var query = from usrRole in _erpContext.UserRole
                        join usrMapping in _erpContext.UserUserRoleMapping on usrRole.Id equals usrMapping.UserRoleId
                        where usrMapping.UserId == id && usrMapping.UserRoleId != Guid.Empty
                        select usrRole;
            return await query.FirstOrDefaultAsync();
        }

        public async Task InsertRole(UserRole entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedOn = DateTime.UtcNow;
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
            return await _erpContext.UserUserRoleMapping.AsNoTracking().ToListAsync();
        }

        public async Task<UserUserRoleMapping> GetUserRoleMappingByUserId(Guid id)
        {
            return await _erpContext.UserUserRoleMapping.FirstOrDefaultAsync(c => c.UserId == id && c.UserRoleId != Guid.Empty);
        }
        public async Task<List<UserUserRoleMapping>> GetUsersByUserRoleId(Guid id)
        {
            return await _erpContext.UserUserRoleMapping.Where(c => c.UserRoleId == id).AsNoTracking().ToListAsync();
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

        #region Select List
        public async Task<List<SelectListItem>> GetAvailableUsers()
        {
            var users = await GetUserList();

            var userList = new List<SelectListItem>
            {
                // show default
                new SelectListItem()
                {
                    Value = Guid.Empty.ToString(),
                    Text = "None"
                }
            };

            foreach (var user in users)
            {
                userList.Add(new SelectListItem()
                {
                    Value = user.Id.ToString(),
                    Text = await EncryptionHelper.DecryptData(user.Username, user.Salt)
                });
            }

            return userList.ToList();
        }
        #endregion

    }
}
