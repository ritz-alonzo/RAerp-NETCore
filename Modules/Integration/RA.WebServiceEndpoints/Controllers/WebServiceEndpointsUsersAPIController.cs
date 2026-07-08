using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RA.Data.Data;
using RA.Data.Domain.Application;
using RA.Data.Domain.Users;
using RA.WebFramework.Extensions;
using RAerp.Controllers.Admin;
using RAerp.Helpers.Security;
using RAerp.Models.UsersModel;
using RAerp.Services.ApplicationSettingServices;
using RAerp.Services.UserServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RA.WebServiceEndpoints.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/users")]
    [ApiVersion("1.0")]
    public class WebServiceEndpointsUsersAPIController : AdminApiController
    {
        #region Constants
        private readonly IUserService _userService;
        private readonly IApplicationSettingService _applicationSettingService;
        private readonly ApplicationSetting _appSettings;
        #endregion

        #region Ctor
        public WebServiceEndpointsUsersAPIController(IUserService userService, 
            IApplicationSettingService applicationSettingService)
        {
            _userService = userService;
            _applicationSettingService = applicationSettingService;
            _appSettings = _applicationSettingService.GetCurrentApplicationSettingAsync().Result;
        }
        #endregion
        [HttpGet, MapToApiVersion("1.0")]
        public async Task<IActionResult> GetUserList([FromQuery] UserSearchModel searchModel)
        {
            var userList = await _userService.GetUserList(
                    searchQuery: searchModel.SearchQuery,
                    createdOn: searchModel.SearchCreatedOn
                );

            List<User> decrpytedUserList = new List<User>();
            if (userList.Any())
            {
                foreach (var user in userList)
                {
                    if (!string.IsNullOrEmpty(user.Email))
                        user.Email = await EncryptionHelper.DecryptData(user.Email, user.Salt);
                    decrpytedUserList.Add(user);
                }
            }

            return Ok(GenerateListResponseAdminModel<User>(HttpStatusCode.OK, "Successfully get user list", decrpytedUserList));
        }

        [HttpGet("{id}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetUser(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Invalid user id"));

            User user = null;
            if (Guid.TryParse(id, out Guid value))
                user = await _userService.GetById(value);
            
            if (user == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Failed to get User, User doesn't exists"));

            return Ok(GenerateResponseAdminModel<User>(HttpStatusCode.OK, "Successfully get user data", user));
        }

        [HttpPost, MapToApiVersion("1.0")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            if (user == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Please fill up necessary values"));

            // Validate values here

            // Get Super Admin Role 
            Guid? superAdminUserId = null;
            var superAdminRole = await _userService.GetUserRoleByRoleNameAsync("Super Admin");
            if (superAdminRole != null)
            {
                var superAdminUserMapping = (await _userService.GetUsersByUserRoleId(superAdminRole.Id)).FirstOrDefault();
                if (superAdminUserMapping != null)
                    superAdminUserId = superAdminUserMapping.UserId;
            }

            user.Salt = EncryptionHelper.GenerateSalt();
            user.Username = user.Username;
            user.Password = await EncryptionHelper.EncryptData(user.Password, user.Salt);
            user.Email = await EncryptionHelper.EncryptData(user.Email, user.Salt);
            user.ContactNo = await EncryptionHelper.EncryptData(user.ContactNo, user.Salt);
            user.CreatedById = superAdminUserId;
            user.AccountStatus = UserAccountStatus.Active;

            await _userService.Insert(user);

            // Get Guest User Role
            var guestUserRole = await _userService.GetUserRoleByRoleNameAsync("Guest");
            if (guestUserRole == null)
                return NotFound();

            await _userService.InsertMapping(user.Id, guestUserRole.Id);

            return Ok(GenerateResponseAdminModel(HttpStatusCode.OK, "Successfully created user, Please login to proceed.", user));
        }

        [HttpPost("changepassword"), MapToApiVersion("1.0")]
        public async Task<IActionResult> UserChangePassword([FromBody] UserModel userModel)
        {
            if (userModel == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Failed to change password, Please fill up necessary values"));

            if (userModel.Id.IsNullOrEmpty() || string.IsNullOrEmpty(userModel.CurrentPassword) || string.IsNullOrEmpty(userModel.NewPassword) || string.IsNullOrEmpty(userModel.ConfirmNewPassword))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Failed to change password, Please fill up necessary values"));

            var user = await _userService.GetById(userModel.Id);
            if (user == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Failed to get user data"));

            bool hasError = false;
            string errMessage = "";
            if (string.IsNullOrEmpty(userModel.CurrentPassword))
            {
                hasError = true;
                errMessage = "Current password is required";
            }
            else if (string.IsNullOrEmpty(userModel.NewPassword))
            {
                hasError = true;
                errMessage = "New password is required";
            }
            else if (string.IsNullOrEmpty(userModel.ConfirmNewPassword))
            {
                hasError = true;
                errMessage = "Confirm new password is required";
            }
            else if (await EncryptionHelper.DecryptData(user.Password, user.Salt) != userModel.CurrentPassword)
            {
                hasError = true;
                errMessage = "Current password is incorrect";
            }
            else if (await EncryptionHelper.DecryptData(user.Password, user.Salt) == userModel.NewPassword)
            {
                hasError = true;
                errMessage = "New password must be different from current password";
            }
            else if (userModel.NewPassword != userModel.ConfirmNewPassword)
            {
                hasError = true;
                errMessage = "New password and confirm new password do not match";
            }

            if (!hasError)
            {
                user.Password = await EncryptionHelper.EncryptData(userModel.NewPassword, user.Salt);
                user.ModifiedOn = DateTime.UtcNow;
                user.LastPasswordChangedDate = DateTime.UtcNow;
                await _userService.Update(user);

                return Ok(GenerateResponseAdminModel<User>(HttpStatusCode.OK, "Successfully updated password", user));
            }
            else
            {
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, errMessage));
            }
        }
    }
}
