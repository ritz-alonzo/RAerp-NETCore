using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Data.Domain.Users;
using RAerp.Models.UsersModel;
using System.Collections.Generic;

namespace RAerp.Factories.UserFactory
{
    public interface IUserModelFactory
    {
        #region Users
        Task<UserModel> PrepareUserModel(UserModel model, User entity);
        Task<UserSearchModel> PrepareUserSearchModel(UserSearchModel searchModel, int pageNumber, int pageSize);
        Task<UserListModel> PrepareUserListModel(UserSearchModel searchModel);
        #endregion

        #region UserRoles
        Task<UserRoleModel> PrepareUserRoleModel(UserRoleModel model, UserRole entity);
        Task<UserRoleSearchModel> PrepareUserRoleSearchModel(UserRoleSearchModel searchModel, int pageNumber, int pageSize);
        Task<UserRoleListModel> PrepareUserRoleListModel(UserRoleSearchModel searchModel);
        Task<List<SelectListItem>> PrepareUserRoleSelectList(bool showDefault = true);
        #endregion
    }
}