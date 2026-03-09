using Microsoft.AspNetCore.Mvc;
using RA.Core.Helpers;
using RAerp.Attributes;
using RAerp.Factories.AccessRightsFactory;
using RAerp.Helpers.Constants;
using RAerp.Models.AccessRightsControlModel;
using RAerp.Security.AccessRights;
using RAerp.Services.AccessRightsServices;
using Newtonsoft.Json;
using RA.WebFramework.Extensions;
using RAerp.Services.UserServices;
using RAerp.Models.UsersModel;
using RA.Data.Domain.AccessRightControl;
using RAerp.Helpers.UserHelper;

namespace RAerp.Controllers.Admin
{
    /// <summary>
    /// This is where all access control of user
    /// </summary>
    public class AccessRightsController : AdminController
    {
        private readonly IAccessRightsModelFactory _accessRightsModelFactory;
        private readonly IAccessRightsService _accessRightsService;
        private readonly IUserService _userService;
        private readonly IUserIdentity _userIdentity;

        public AccessRightsController(IAccessRightsModelFactory accessRightsModelFactory,
            IAccessRightsService accessRightsService,
            IUserService userService,
            IUserIdentity userIdentity)
        {
            _accessRightsModelFactory = accessRightsModelFactory;
            _accessRightsService = accessRightsService;
            _userService = userService;
            _userIdentity = userIdentity;
        }

        //[AccountLoggedOnAuthentication]
        public async Task<IActionResult> Index()
        {
            // will add here checking of current user if super admin
            var model = await _accessRightsModelFactory.PrepareAccessRightsSearchModel(new AccessRightsSearchModel(), 1, int.MaxValue);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Save(string recordName,
            string systemName, 
            string classification, 
            int accessTypeId, 
            string roleId, 
            bool isChecked)
        {
            if (string.IsNullOrEmpty(roleId))
                return JsonError(AdminErrorMessages.UserRoleIdNotExists);

            if (string.IsNullOrEmpty(recordName) || string.IsNullOrEmpty(systemName) || string.IsNullOrEmpty(classification) || accessTypeId == 0)
                return JsonError("Incomplete details of Access Record");

            var userRoleId = Guid.Parse(roleId);

            if (userRoleId.IsNullOrEmpty())
                return JsonError(AdminErrorMessages.UserRoleIdNotExists);

            var userRole = await _userService.GetUserRoleById(userRoleId);
            var accessRights = await _accessRightsService.GetAccessRightsByUserRoleId(userRoleId);
            var accessRecordSystemNameList = new List<string>();

            #region Assign Access Record Values
            AccessRecord accessRecord = new AccessRecord();
            accessRecord.SystemName = systemName;
            accessRecord.Classification = classification;
            accessRecord.Name = recordName;
            switch (accessTypeId)
            {
                case (int)AccessType.View:
                    accessRecord.AccessRecordType = AccessType.View;
                    break;

                case (int)AccessType.Create:
                    accessRecord.AccessRecordType = AccessType.Create;
                    break;

                case (int)AccessType.Update:
                    accessRecord.AccessRecordType = AccessType.Update;
                    break;

                case (int)AccessType.Delete:
                    accessRecord.AccessRecordType = AccessType.Delete;
                    break;

                case (int)AccessType.Manage:
                    accessRecord.AccessRecordType = AccessType.Manage;
                    break;

                case (int)AccessType.Custom:
                    accessRecord.AccessRecordType = AccessType.Custom;
                    break;

                default:
                    break;
            }
            #endregion

            #region Insert or Update
            // insert
            if (isChecked)
            {
                if (accessRights == null)
                {
                    // prepare access rights data
                    accessRecordSystemNameList.Add(accessRecord.SystemName);
                    accessRights = new AccessRights();
                    // add created by id
                    accessRights.CreatedById = (await _userIdentity.GetCurrentUserAsync(HttpContext)).Id;
                    accessRights.AccessRecordData = JsonConvert.SerializeObject(accessRecordSystemNameList);
                    accessRights.UserRoleId = userRoleId;
                    accessRights.Rolename = userRole.Rolename;
                    
                    await _accessRightsService.Insert(accessRights);
                }
                else
                {
                    if (accessRights.AccessRecordData != null && accessRights.AccessRecordData.IsNotNullOrEmptyJson())
                        accessRecordSystemNameList = JsonConvert.DeserializeObject<List<string>>(accessRights.AccessRecordData);
                    accessRecordSystemNameList.Add(accessRecord.SystemName);
                    accessRights.AccessRecordData = JsonConvert.SerializeObject(accessRecordSystemNameList);
                    // add modified by id
                    accessRights.ModifiedById = (await _userIdentity.GetCurrentUserAsync(HttpContext)).Id;
                    await _accessRightsService.Update(accessRights);
                }
            }
            // update accessRecordData to remove the access record
            else
            {
                // sanity check
                if (accessRights == null)
                {
                    return JsonError(AdminErrorMessages.AccessRightsNotExists);
                }
                else
                {
                    accessRecordSystemNameList = JsonConvert.DeserializeObject<List<string>>(accessRights.AccessRecordData);
                    if (accessRecordSystemNameList.Any())
                    {
                        accessRecordSystemNameList = accessRecordSystemNameList.Where(c => !c.Contains(accessRecord.SystemName)).ToList();
                        if (accessRecordSystemNameList.Any())
                            accessRights.AccessRecordData = JsonConvert.SerializeObject(accessRecordSystemNameList);
                        else
                            accessRights.AccessRecordData = "{}";
                        accessRights.ModifiedById = (await _userIdentity.GetCurrentUserAsync(HttpContext)).Id;
                        await _accessRightsService.Update(accessRights);
                    }
                }
            }
            #endregion

            return NullJsonResult();
        }

        // working table filter
        // will need to update preparing User list
        [HttpGet]
        public async Task<IActionResult> AccessRightsList(AccessRightsSearchModel searchModel)
        {
            var model = await _accessRightsModelFactory.PrepareAccessRightsListModel(searchModel);

            return PartialView("_AccessRightsList", model);
        }

        // add Unathorize View here
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
