#region namespaces
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RA.Data.Data;
using RA.WebFramework.Extensions;
using RAerp.Controllers.Admin;
using RAerp.Domain.Application;
using RAerp.Domain.Users;
using RAerp.DTO.Users;
using RAerp.Helpers.Constants;
using RAerp.Helpers.OTPHelper;
using RAerp.Helpers.Security;
using RAerp.Helpers.UserHelper;
using RAerp.Models.EmailModel;
using RAerp.Models.UsersModel;
using RAerp.Services.ApplicationSettingServices;
using RAerp.Services.EmailServices;
using RAerp.Services.UserServices;
using System.Net;
#endregion

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
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly IUserIdentity _userIdentity;
        #endregion

        #region Ctor
        public WebServiceEndpointsUsersAPIController(IUserService userService,
            IApplicationSettingService applicationSettingService,
            IEmailService emailService,
            IMapper mapper,
            IUserIdentity userIdentity)
        {
            _userService = userService;
            _applicationSettingService = applicationSettingService;
            _appSettings = _applicationSettingService.GetCurrentApplicationSettingAsync().Result;
            _emailService = emailService;
            _mapper = mapper;
            _userIdentity = userIdentity;
        }
        #endregion

        [HttpGet, MapToApiVersion("1.0")]
        [Authorize]
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

        [HttpGet("{id:guid}"), MapToApiVersion("1.0")]
        [Authorize]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            if (id.IsNullOrEmpty())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Invalid user id"));

            User user = await _userService.GetById(id);
            if (user == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Failed to get User, User doesn't exists"));

            UserResponseDto userResponse = _mapper.Map<UserResponseDto>(user);
            userResponse.Email = await EncryptionHelper.DecryptData(user.Email, user.Salt);
            userResponse.ContactNo = await EncryptionHelper.DecryptData(user.ContactNo, user.Salt);

            return Ok(GenerateResponseModel(HttpStatusCode.OK, "Successfully get user data", userResponse));
        }

        [HttpPost, MapToApiVersion("1.0")]
        public async Task<IActionResult> CreateUser([FromBody] UserRequestDto userRequest)
        {
            if (userRequest == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Please fill up necessary values"));
            // Get Super Admin Role 
            Guid? superAdminUserId = null;
            var superAdminRole = await _userService.GetUserRoleByRoleNameAsync("Super Admin");
            if (superAdminRole != null)
            {
                var superAdminUserMapping = (await _userService.GetUsersByUserRoleId(superAdminRole.Id)).FirstOrDefault();
                if (superAdminUserMapping != null)
                    superAdminUserId = superAdminUserMapping.UserId;
            }

            if (await _userService.CheckUserIfExists(userRequest.Username, userRequest.Email))
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "User with the same username or email already exists"));
            
            User user = _mapper.Map<User>(userRequest);

            if (_appSettings.IsEmailVerificationEnabled)
            {
                user.AccountStatus = UserAccountStatus.Pending;
                user.OneTimePIN = OneTimePINHelper.GenerateOTP();
                user.OneTimePINValidUntil = DateTime.UtcNow.AddMinutes(30);
                // Send OTP to user email
                var subject = "Email Verification";
                var htmlContent = string.Format(AdminMessages.EmailVerificationHtmlContent, 
                    user?.FirstName, 
                    user?.LastName, 
                    user?.OneTimePINValidUntil.ConvertUTCToAppSettingsDateTime(_appSettings.DefaultTimeZone).ToLongDateString() + ", " + user?.OneTimePINValidUntil.ConvertUTCToAppSettingsDateTime(_appSettings.DefaultTimeZone).ToLongTimeString(), 
                    user.OneTimePIN);
                BrevoEmailResponseModel emailResponseModel = await _emailService.SendBrevoEmailAsync(user?.Email, user?.FirstName + " " + user?.LastName, subject, htmlContent);
                if (string.IsNullOrEmpty(emailResponseModel.MessageId))
                    return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Failed to send OTP to user email"));
            }
            else
                user.AccountStatus = UserAccountStatus.Active;

            user.Salt = EncryptionHelper.GenerateSalt();
            user.Username = user.Username;
            user.Password = await EncryptionHelper.EncryptData(user.Password, user.Salt);
            user.Email = await EncryptionHelper.EncryptData(user.Email, user.Salt);
            user.ContactNo = await EncryptionHelper.EncryptData(user.ContactNo, user.Salt);
            user.CreatedById = superAdminUserId;

            await _userService.Insert(user);

            // Get Guest User Role
            var guestUserRole = await _userService.GetUserRoleByRoleNameAsync("Guest");
            if (guestUserRole == null)
                return NotFound();

            await _userService.InsertMapping(user.Id, guestUserRole.Id);

            return Ok("Successfully created user, Please login to proceed.");
        }

        [HttpPost("resendotp"), MapToApiVersion("1.0")]
        public async Task<IActionResult> ResendEmailOTP([FromBody] UserRequestDto userRequest)
        {
            if (userRequest == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Please fill up necessary values"));
            var existingUser = await _userService.GetUserByUsername(userRequest?.Username);
            if (existingUser == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Invalid username or password"));
            
            if (existingUser.AccountStatus == UserAccountStatus.Active)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "User account is already active"));

            if (!_appSettings.IsEmailVerificationEnabled)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Email verification is not enabled"));

            if (existingUser.OneTimePINAttempt.GetValueOrDefault() >= _appSettings.OneTimePINAttemptLimit)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Maximum OTP attempts exceeded"));

            if (existingUser.EmailResendAttempt.GetValueOrDefault() >= _appSettings.EmailVerificationAttemptLimit)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Maximum OTP email resend attempts exceeded"));

            string existingUserEmail = await EncryptionHelper.DecryptData(existingUser?.Email, existingUser.Salt);
            if (!string.IsNullOrEmpty(existingUserEmail) && existingUserEmail != userRequest.Email)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Email is not matched to registered email"));

            existingUser.OneTimePIN = OneTimePINHelper.GenerateOTP();
            existingUser.OneTimePINValidUntil = DateTime.UtcNow.AddMinutes(30);
            existingUser.EmailResendAttempt = (existingUser.EmailResendAttempt ?? 0) + 1;
            await _userService.Update(existingUser);
            // Send OTP to user email
            var subject = "Email Verification";
            var htmlContent = string.Format(AdminMessages.EmailVerificationHtmlContent,
                                existingUser?.FirstName,
                                existingUser?.LastName,
                                existingUser?.OneTimePINValidUntil.ConvertUTCToAppSettingsDateTime(_appSettings.DefaultTimeZone).ToLongDateString() + ", " + existingUser?.OneTimePINValidUntil.ConvertUTCToAppSettingsDateTime(_appSettings.DefaultTimeZone).ToLongTimeString(),
                                existingUser.OneTimePIN);
            BrevoEmailResponseModel emailResponseModel = await _emailService.SendBrevoEmailAsync(existingUserEmail, existingUser?.FirstName + " " + existingUser?.LastName, subject, htmlContent);
            if (string.IsNullOrEmpty(emailResponseModel.MessageId))
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Failed to send OTP to user email"));

            return Ok("Successfully resent OTP to user email");
        }

        [HttpPost("validate"), MapToApiVersion("1.0")]
        public async Task<IActionResult> ValidateUser([FromBody] UserRequestDto userRequest)
        {
            if (userRequest == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Please fill up necessary values"));

            var existingUser = await _userService.GetUserByUsername(userRequest?.Username);
            if (existingUser == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Invalid username or password"));

            if (existingUser.AccountStatus == UserAccountStatus.Active)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "User account is already active"));

            if (!_appSettings.IsEmailVerificationEnabled)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Email verification is not enabled"));

            if (string.IsNullOrEmpty(existingUser.OneTimePIN))
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "OTP is required"));

            if (existingUser.OneTimePIN != userRequest.OneTimePIN)
            {
                existingUser.OneTimePINAttempt = (existingUser.OneTimePINAttempt ?? 0) + 1;
                await _userService.Update(existingUser);
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, $"Invalid OTP, please try again. You have {_appSettings.OneTimePINAttemptLimit - existingUser.OneTimePINAttempt} attempts remaining"));
            }

            if (existingUser.OneTimePINAttempt > _appSettings.OneTimePINAttemptLimit)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Maximum OTP attempt reached"));

            if (existingUser.OneTimePINValidUntil.ConvertUTCToAppSettingsDateTime(_appSettings.DefaultTimeZone) < DateTime.UtcNow)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "OTP has expired"));

            existingUser.AccountStatus = UserAccountStatus.Active;
            existingUser.OneTimePIN = null;
            existingUser.OneTimePINValidUntil = null;
            existingUser.OneTimePINAttempt = null;
            existingUser.EmailResendAttempt = null;
            existingUser.IsVerified = true;
            await _userService.Update(existingUser);

            return Ok("Successfully validated user, you may proceed to login");
        }

        [HttpPost("changepassword"), MapToApiVersion("1.0")]
        [Authorize]
        public async Task<IActionResult> UserChangePassword([FromBody] UserChangePasswordRequestDto userRequest)
        {
            if (userRequest == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Failed to change password, Please fill up necessary values"));

            if (userRequest.Id.IsNullOrEmpty() || string.IsNullOrEmpty(userRequest.CurrentPassword) || string.IsNullOrEmpty(userRequest.NewPassword) || string.IsNullOrEmpty(userRequest.ConfirmNewPassword))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Failed to change password, Please fill up necessary values"));

            var user = await _userService.GetById(userRequest.Id);
            if (user == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Failed to get user data"));

            bool hasError = false;
            string errMessage = "";
            if (string.IsNullOrEmpty(userRequest.CurrentPassword))
            {
                hasError = true;
                errMessage = "Current password is required";
            }
            else if (string.IsNullOrEmpty(userRequest.NewPassword))
            {
                hasError = true;
                errMessage = "New password is required";
            }
            else if (string.IsNullOrEmpty(userRequest.ConfirmNewPassword))
            {
                hasError = true;
                errMessage = "Confirm new password is required";
            }
            else if (await EncryptionHelper.DecryptData(user.Password, user.Salt) != userRequest.CurrentPassword)
            {
                hasError = true;
                errMessage = "Current password is incorrect";
            }
            else if (await EncryptionHelper.DecryptData(user.Password, user.Salt) == userRequest.NewPassword)
            {
                hasError = true;
                errMessage = "New password must be different from current password";
            }
            else if (userRequest.NewPassword != userRequest.ConfirmNewPassword)
            {
                hasError = true;
                errMessage = "New password and confirm new password do not match";
            }

            if (!hasError)
            {
                user.Password = await EncryptionHelper.EncryptData(userRequest.NewPassword, user.Salt);
                user.ModifiedOn = DateTime.UtcNow;
                user.LastPasswordChangedDate = DateTime.UtcNow;
                await _userService.Update(user);

                return Ok("Successfully updated password");
            }
            else
            {
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, errMessage));
            }
        }

        [HttpGet("current"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetCurrentUser()
        {
            User currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);
            if (currentUser == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Failed to get current user, User id is invalid"));
            
            UserRole userRole = await _userService.GetUserRoleByUserId(currentUser.Id);
            if (userRole == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Failed to get current user role, User role is invalid"));
            
            return Ok(new
            {
                Status = HttpStatusCode.OK.ToString(),
                Role = userRole.Rolename,
                Username = currentUser.Username,
                FullName = currentUser?.FirstName + " " + currentUser?.LastName,
            });
        }
    }
}
