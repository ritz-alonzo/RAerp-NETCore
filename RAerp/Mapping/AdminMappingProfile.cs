using AutoMapper;
using RAerp.Domain.Application;
using RAerp.Domain.EntityAttributes;
using RAerp.Domain.Users;
using RAerp.DTO.EntityAttributes;
using RAerp.DTO.Users;
using RAerp.Models.AccessRightsControlModel;
using RAerp.Models.ApplicationSettingsModel;
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

            #region ApplicationSetting

            CreateMap<ApplicationSetting, ApplicationSettingModel>()
                .ReverseMap();

            #endregion

            #region Entity Attributes Dto
            CreateMap<EntityAttribute, EntityAttributeResponseDto>();
            CreateMap<EntityAttributeRequestDto, EntityAttribute>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion

            #region User Dto
            CreateMap<User, UserResponseDto>();
            CreateMap<UserRequestDto, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            #endregion
        }
    }
}
