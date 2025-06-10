using AutoMapper;
using RA.Data.Domain.Users;
using RAerp.Models.AccessRightsControlModel;
using RAerp.Models.UsersModel;
using RAerp.Security.AccessRights;

namespace RAerp.Mapping
{
    public class AdminMappingProfile : Profile
    {
        public AdminMappingProfile()
        {
            #region Users
            CreateMap<User, UserModel>();
            CreateMap<UserModel, User>();
            #endregion

            #region User Roles
            CreateMap<UserRole, UserRoleModel>();
            CreateMap<UserRoleModel, UserRole>();
            #endregion

            #region AccessRights
            CreateMap<AccessRecord, AccessRightsModel>();
            CreateMap<AccessRightsModel, AccessRecord>();
            #endregion

        }
    }
}
