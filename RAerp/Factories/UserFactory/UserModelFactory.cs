using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RA.Core.Helpers;
using RA.Data.Data;
using RA.Data.Domain.Users;
using RA.WebFramework.Extensions;
using RAerp.Factories.CoreFactories;
using RAerp.Helpers.Constants;
using RAerp.Helpers.UserHelper;
using RAerp.Models.UsersModel;
using RAerp.Services.UserServices;
using System.Security.Cryptography.Xml;

namespace RAerp.Factories.UserFactory
{
    /// <summary>
    /// Preparing model of User
    /// will add form behaviors in the future.
    /// </summary>
    public class UserModelFactory : IUserModelFactory
    {
        private readonly IUserService _userService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IMapper _mapper;
        private readonly IUserIdentity _userIdentity;

        public UserModelFactory(IUserService userService, 
            IBaseAdminModelFactory baseSearchModelFactory, 
            IMapper mapper, 
            IUserIdentity userIdentity)
        {
            _userService = userService;
            _baseAdminModelFactory = baseSearchModelFactory;
            _mapper = mapper;
            _userIdentity = userIdentity;
        }

        #region Users

        public virtual async Task<UserSearchModel> PrepareUserSearchModel(UserSearchModel searchModel, int pageNumber, int pageSize)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            _baseAdminModelFactory.PrepareBaseAdminSearchModel(searchModel, pageSize, pageNumber);

            searchModel.SystemName = typeof(User).FullName;
            searchModel.Users = await PrepareUserListModel(searchModel);
            searchModel.TotalItems = (int)searchModel.Users.TotalItems;
            searchModel.PageSize = searchModel.Users.PageSize;

            return searchModel;
        }

        public virtual async Task<UserListModel> PrepareUserListModel(UserSearchModel searchModel)
        {
            var model = new UserListModel();

            var userList = await _userService.GetUserList(
                searchQuery: searchModel.SearchQuery,
                createdOn: searchModel.SearchCreatedOn);

            var users = new List<UserModel>();

            users = userList.Select(user =>
            {
                var userModel = new UserModel();
                // automapper working from entity to model
                // needed to do this to prevent tracking error in update
                // source : user destination : userModel
                // needed to create new UserModel() first
                userModel = _mapper.Map(user, userModel);

                return userModel;

            }).ToList();

            _baseAdminModelFactory.PrepareBaseAdminListModel(model, users, searchModel.PageSize, searchModel.PageNumber, searchModel.Skip, userList.Count());

            return model;
        }

        public virtual async Task<UserModel> PrepareUserModel(UserModel model, User entity)
        {
            if (model == null)
                throw new ArgumentNullException(AdminErrorMessages.UserNotExists);

            if (entity == null)
            {
                entity = new User();

                entity.AccountStatus = UserAccountStatus.Active;
                entity.CreatedOn = DateTime.Now;
                // but for now disable
                //entity.CreatedById = _userIdentity.GetCurrentUser(_httpContextAccessor.HttpContext)?.Id;
            }

            model = _mapper.Map(entity, model);

            if (entity.ModifiedOn.HasValue)
            {
                model.ModifiedOn = entity.ModifiedOn;
            }

            model.AccountStatus = entity.AccountStatus;
            // will fix this so that when there's still no user role available will
            // automatically create Admin role.
            // already done in controller will move it to service.
            model.AvailableUserRoles = await PrepareUserRoleSelectList();

            if (entity.Id.IsNotNullOrEmpty())
            {
                // user mappings
                var userIsMapped = await _userService.GetUserRoleMappingByUserId(entity.Id);
                if (userIsMapped != null)
                {
                    var userRole = await _userService.GetUserRoleById(userIsMapped.UserRoleId);
                    var selectedUserRole = model.AvailableUserRoles.Where(c => c.Value == userRole.Id.ToString()).FirstOrDefault();
                    selectedUserRole.Selected = true;
                    model.UserRoleId = userRole.Id;
                }

                // change mapped password to any password
                model.Password = AdminMessages.HiddenPasswordDisplay;
            }

            _baseAdminModelFactory.PrepareBaseAdminModel(model, entity);

            return model;
        }

        #endregion

        #region User Roles

        public virtual async Task<UserRoleSearchModel> PrepareUserRoleSearchModel(UserRoleSearchModel searchModel, int pageNumber, int pageSize)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            _baseAdminModelFactory.PrepareBaseAdminSearchModel(searchModel, pageSize, pageNumber);

            searchModel.SystemName = typeof(UserRole).FullName;
            searchModel.UserRoles = await PrepareUserRoleListModel(searchModel);
            searchModel.TotalItems = (int)searchModel.UserRoles.TotalItems;
            searchModel.PageSize = searchModel.UserRoles.PageSize;

            return searchModel;
        }

        public virtual async Task<UserRoleListModel> PrepareUserRoleListModel(UserRoleSearchModel searchModel)
        {
            var model = new UserRoleListModel();

            var userRoleList = await _userService.GetUserRoleList(
                searchQuery: searchModel.SearchQuery,
                createdOn: searchModel.SearchCreatedOn);

            var userRoles = new List<UserRoleModel>();

            userRoles = userRoleList.Select(userRole =>
            {
                var userRoleModel = new UserRoleModel();
                // automapper working from entity to model
                // needed to do this to prevent tracking error in update
                // source : user destination : userModel
                // needed to create new UserModel() first
                userRoleModel = _mapper.Map(userRole, userRoleModel);

                return userRoleModel;

            }).ToList();

            _baseAdminModelFactory.PrepareBaseAdminListModel(model, userRoles, searchModel.PageSize, searchModel.PageNumber, searchModel.Skip, userRoleList.Count());

            return model;
        }

        public virtual async Task<UserRoleModel> PrepareUserRoleModel(UserRoleModel model, UserRole entity)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            
            if (entity == null)
            {
                entity = new UserRole();

                entity.CreatedOn = DateTime.Now;
            }

            model = _mapper.Map(entity, model);

            if (entity.ModifiedOn.HasValue)
            {
                model.ModifiedOn = entity.ModifiedOn;
            }

            _baseAdminModelFactory.PrepareBaseAdminModel(model, entity);

            return model;
        }

        public virtual async Task<List<SelectListItem>> PrepareUserRoleSelectList(bool showDefault = true)
        {
            var userRoleSelectList = new List<SelectListItem>();

            if (showDefault)
            {
                userRoleSelectList.Insert(0, new SelectListItem()
                {
                    Text = AdminMessages.SelectListNoneValue,
                    Value = Guid.Empty.ToString()
                });
            }

            var availableUserRoles = await _userService.GetUserRoleList();
            if (!availableUserRoles.Any())
            {
                var adminUserRole = new UserRole()
                {
                    Rolename = AdminMessages.AdminRole
                };
                await _userService.InsertRole(adminUserRole);

                userRoleSelectList.Add(new SelectListItem
                {
                    Value = adminUserRole.Id.ToString(),
                    Text = adminUserRole.Rolename,
                });
            }
            else
            {
                if (!availableUserRoles.Any(c => c.Rolename.Equals(AdminMessages.AdminRole)))
                {
                    var adminUserRole = new UserRole()
                    {
                        Rolename = AdminMessages.AdminRole
                    };
                    await _userService.InsertRole(adminUserRole);

                    userRoleSelectList.Add(new SelectListItem
                    {
                        Value = adminUserRole.Id.ToString(),
                        Text = adminUserRole.Rolename,
                    });
                }

                foreach (var userRole in availableUserRoles)
                {
                    // do not add super admin role
                    if (userRole.Rolename == AdminMessages.SuperAdminRole)
                        continue;

                    userRoleSelectList.Add(new SelectListItem
                    {
                        Text = userRole.Rolename,
                        Value = userRole.Id.ToString()
                    });
                }
            }

            return userRoleSelectList;
        }

        #endregion
    }
}
