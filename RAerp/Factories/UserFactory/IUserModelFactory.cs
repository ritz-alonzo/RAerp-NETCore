using Microsoft.AspNetCore.Mvc.Rendering;
using RAerp.Domain.Users;
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
        Task<UserRoleModel> PrepareUserRoleModelAsync(UserRoleModel model, UserRole entity);
        Task<UserRoleSearchModel> PrepareUserRoleSearchModelAsync(UserRoleSearchModel searchModel, int pageNumber, int pageSize);
        Task<UserRoleListModel> PrepareUserRoleListModelAsync(UserRoleSearchModel searchModel);
        Task<List<SelectListItem>> PrepareUserRoleSelectListAsync(bool showDefault = true);
        #endregion
    }
}