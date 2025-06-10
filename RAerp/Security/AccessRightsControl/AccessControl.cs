using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RA.Core.Helpers;
using RA.Data.App_Data;
using RA.Data.Domain.Users;
using RA.WebFramework.Extensions;
using RAerp.Helpers.Constants;
using RAerp.Helpers.Security;
using RAerp.Helpers.UserHelper;
using RAerp.Security.AccessRights;
using RAerp.Services.AccessRightsServices;

namespace RAerp.Security.AccessRightsControl
{
    public class AccessControl : IAccessControl
    {
        private readonly IAccessRightsService _accessRightsService;
        private readonly IUserIdentity _userIdentity;
        private readonly RAerpContext _erpContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccessControl(IAccessRightsService accessRightsService,
            IUserIdentity userIdentity,
            RAerpContext erpContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _accessRightsService = accessRightsService;
            _userIdentity = userIdentity;
            _erpContext = erpContext;
            _httpContextAccessor = httpContextAccessor;
        }

        #region CRUD Access

        #region View

        public async Task<bool> HasViewAccess<TEntity>()
            where TEntity : class
        {
            if (!SessionHelper.SessionGenerated(_httpContextAccessor.HttpContext))
                return false;

            var userId = _userIdentity.GetCurrentUser(_httpContextAccessor.HttpContext)?.Id;

            if (userId.IsNullOrEmpty())
                throw new ArgumentNullException(AdminErrorMessages.UserIdNotExists);

            var userRole = (from role in _erpContext.UserRole
                            join mapping in _erpContext.UserUserRoleMapping
                            on role.Id equals mapping.UserRoleId
                            where mapping.UserId == userId
                            select role).FirstOrDefault();

            if (userRole == null)
                throw new ArgumentNullException(AdminErrorMessages.UserRoleNotExists);

            return await CheckRoleAccessRecord(userRole.Id, AccessType.View, typeof(TEntity));
        }
        #endregion

        #region Create

        public async Task<bool> HasCreateAccess<TEntity>()
            where TEntity : class
        {
            if (!SessionHelper.SessionGenerated(_httpContextAccessor.HttpContext))
                return false;

            var userId = _userIdentity.GetCurrentUser(_httpContextAccessor.HttpContext)?.Id;

            if (userId.IsNullOrEmpty())
                throw new ArgumentNullException(AdminErrorMessages.UserIdNotExists);

            var userRole = (from role in _erpContext.UserRole
                            join mapping in _erpContext.UserUserRoleMapping
                            on role.Id equals mapping.UserRoleId
                            where mapping.UserId == userId
                            select role).FirstOrDefault();

            if (userRole == null)
                throw new ArgumentNullException(AdminErrorMessages.UserRoleNotExists);

            return await CheckRoleAccessRecord(userRole.Id, AccessType.Create, typeof(TEntity));
        }
        #endregion

        #region Update

        public async Task<bool> HasUpdateAccess<TEntity>()
            where TEntity : class
        {
            if (!SessionHelper.SessionGenerated(_httpContextAccessor.HttpContext))
                return false;

            var userId = _userIdentity.GetCurrentUser(_httpContextAccessor.HttpContext)?.Id;

            if (userId.IsNullOrEmpty())
                throw new ArgumentNullException(AdminErrorMessages.UserIdNotExists);

            var userRole = (from role in _erpContext.UserRole
                            join mapping in _erpContext.UserUserRoleMapping
                            on role.Id equals mapping.UserRoleId
                            where mapping.UserId == userId
                            select role).FirstOrDefault();

            if (userRole == null)
                throw new ArgumentNullException(AdminErrorMessages.UserRoleNotExists);

            return await CheckRoleAccessRecord(userRole.Id, AccessType.Update, typeof(TEntity));
        }
        #endregion

        #region Delete

        public async Task<bool> HasDeleteAccess<TEntity>()
            where TEntity : class
        {
            if (!SessionHelper.SessionGenerated(_httpContextAccessor.HttpContext))
                return false;

            var userId = _userIdentity.GetCurrentUser(_httpContextAccessor.HttpContext)?.Id;

            if (userId.IsNullOrEmpty())
                throw new ArgumentNullException(AdminErrorMessages.UserIdNotExists);

            var userRole = (from role in _erpContext.UserRole
                            join mapping in _erpContext.UserUserRoleMapping
                            on role.Id equals mapping.UserRoleId
                            where mapping.UserId == userId
                            select role).FirstOrDefault();

            if (userRole == null)
                throw new ArgumentNullException(AdminErrorMessages.UserRoleNotExists);

            return await CheckRoleAccessRecord(userRole.Id, AccessType.Delete, typeof(TEntity));
        }
        #endregion

        #endregion

        #region Access Rights Checking w/o Access Record

        private async Task<bool> CheckRoleAccessRecord(Guid userRoleId, AccessType accessRecordType, Type moduleType)
        {
            if (moduleType == null)
                throw new ArgumentNullException("Type cannot be empty");

            if (string.IsNullOrEmpty(moduleType.FullName))
                throw new ArgumentNullException("System name cannot be empty");

            var accessRights = await _accessRightsService.GetAccessRightsByUserRoleId(userRoleId);

            if (accessRights == null)
                return false;

            if (accessRights.AccessRecordData.IsNullOrEmptyJson())
                return false;
            // Deserialize access right record data
            var accessRecordSystemNameList = JsonConvert.DeserializeObject<List<string>>(accessRights.AccessRecordData);

            if (!accessRecordSystemNameList.Any())
                return false;

            var arType = moduleType.Assembly.GetTypes().Where(c => c.Name.Contains("AccessRightsRecord") && c.IsClass).FirstOrDefault();
            if (arType == null)
                return false;

            var arrFields = arType.GetFields().Where(c => c.FieldType == typeof(AccessRecord)).ToList();
            if (!arrFields.Any())
                return false;

            var ARRInstance = Activator.CreateInstance(arType);

            var accessRecordByAccessType = arrFields.Select(c => c.GetValue(ARRInstance) as AccessRecord).FirstOrDefault(c => c.AccessRecordType == accessRecordType);
            if (accessRecordByAccessType == null)
                return false;
            // will add switch in case there's complexity
            // but for now use this
            // needed to add name for identification of access record
            return accessRecordSystemNameList.Where(c => c == accessRecordByAccessType.SystemName).FirstOrDefault() == null ? false : true;
        }

        #endregion

        #region Direct checking of Access with Access Record

        public async Task<bool> AccessPermitted<TEntity>(AccessRecord accessRecord)
            where TEntity : class
        {
            if (!SessionHelper.SessionGenerated(_httpContextAccessor.HttpContext))
                return false;

            if (accessRecord == null)
                return false;

            var userId = _userIdentity.GetCurrentUser(_httpContextAccessor.HttpContext)?.Id;

            if (userId.IsNullOrEmpty())
                throw new ArgumentNullException(AdminErrorMessages.UserIdNotExists);

            var userRole = (from role in _erpContext.UserRole
                            join mapping in _erpContext.UserUserRoleMapping
                            on role.Id equals mapping.UserRoleId
                            where mapping.UserId == userId
                            select role).FirstOrDefault();

            if (userRole == null)
                throw new ArgumentNullException(AdminErrorMessages.UserRoleNotExists);

            var accessRights = await _accessRightsService.GetAccessRightsByUserRoleId(userRole.Id);

            if (accessRights == null)
                return false;

            if (accessRights.AccessRecordData.IsNullOrEmptyJson())
                return false;

            // Deserialize access right record data
            var accessRecordSystemNameList = JsonConvert.DeserializeObject<List<string>>(accessRights.AccessRecordData);
            if (!accessRecordSystemNameList.Any())
                return false;

            return accessRecordSystemNameList.Contains(accessRecord.SystemName);
        }

        #endregion

        #region Super Admin Access

        public bool HasSuperAdminAccess()
        {
            if (!SessionHelper.SessionGenerated(_httpContextAccessor.HttpContext))
                return false;

            var userId = _userIdentity.GetCurrentUser(_httpContextAccessor.HttpContext)?.Id;

            if (userId.IsNullOrEmpty())
                throw new ArgumentNullException(AdminErrorMessages.UserIdNotExists);

            var userRole = (from role in _erpContext.UserRole
                            join mapping in _erpContext.UserUserRoleMapping
                            on role.Id equals mapping.UserRoleId
                            where mapping.UserId == userId
                            select role).FirstOrDefault();

            if (userRole == null) 
                throw new ArgumentNullException(AdminErrorMessages.UserRoleNotExists);

            if (userRole.Rolename != AdminMessages.SuperAdminRole)
                return false;

            return true;
        }

        #endregion
    }
}
