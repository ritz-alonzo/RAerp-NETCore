#region namespaces
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RA.Data.App_Data;
using RA.Data.Data;
using RA.Data.Domain.Application;
using RA.Data.Domain.Users;
using RA.WebFramework.Extensions;
using RAerp.Attributes;
using RAerp.Controllers.Admin;
using RAerp.Data.Security;
using RAerp.Factories.UserFactory;
using RAerp.Helpers.Constants;
using RAerp.Helpers.PluginHelper;
using RAerp.Helpers.Security;
using RAerp.Helpers.SMSHelper;
using RAerp.Models.UsersModel;
using RAerp.Security.AccessRights;
using RAerp.Security.AccessRightsControl;
using RAerp.Services.AddressServices;
using RAerp.Services.ApplicationSettingServices;
using RAerp.Services.EmailServices;
using RAerp.Services.UserServices;
using System.Reflection;
#endregion

namespace RAErp.Controllers.Users
{
    [AutoValidateAntiforgeryToken]
    public class UsersController : AdminController
    {
        #region Constants
        private readonly RAerpContext _erpContext;
        private readonly IUserService _userService;
        private readonly IUserModelFactory _userModelFactory;
        private readonly IMapper _mapper;
        private readonly IAccessControl _accessControl;
        private readonly IAddressService _addressService;
        private readonly string _smsAPIKey;
        private readonly IEmailService _emailService;
        private readonly IApplicationSettingService _applicationSettingService;
        private readonly ApplicationSetting _applicationSetting;
        #endregion

        #region Ctor
        public UsersController(RAerpContext erpContext,
            IUserService userService,
            IUserModelFactory userModelFactory,
            IMapper mapper,
            IAccessControl accessControl,
            IAddressService addressService,
            IConfiguration configuration,
            IEmailService emailService,
            IApplicationSettingService applicationSettingService)
        {
            _erpContext = erpContext;
            _userService = userService;
            _userModelFactory = userModelFactory;
            _mapper = mapper;
            _accessControl = accessControl;
            _addressService = addressService;
            _smsAPIKey = configuration.GetValue<string>("ApiSettings:SMSApiKey");
            _emailService = emailService;
            _applicationSettingService = applicationSettingService;
            _applicationSetting = _applicationSettingService.GetCurrentApplicationSettingAsync().Result;
        }
        #endregion

        #region Users CRUD
        //[AccountLoggedOnAuthentication]
        public async Task<IActionResult> List(int page = 1)
        {
            // working, will add this to every action of Super Admin pages
            //if (!_accessControl.HasSuperAdminAccess())
            //    return UnauthorizedAccess();

            var model = await _userModelFactory.PrepareUserSearchModel(new UserSearchModel(), page, 10);

            return View(model);
        }

        // working table filter
        // will need to update preparing User list
        [HttpGet]
        public async Task<IActionResult> UserListSearch(UserSearchModel searchModel)
        {
            var model = await _userModelFactory.PrepareUserListModel(searchModel);

            return PartialView("_UserList", model);
        }

        // will enable this base on system settings
        // default will use Create
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var model = new UserModel();
            // add permission here if user can create
            // add prepare model in create
            model.AvailableUserRoles = await _userModelFactory.PrepareUserRoleSelectListAsync(showDefault: false);

            return View(model);
        }

        // will enable this base on system settings
        [HttpPost]
        public async Task<IActionResult> Register(UserModel model)
        {
            if (ModelState.IsValid)
            {
                var userSalt = EncryptionHelper.GenerateSalt();
                // convert model to entity using auto mapper
                var entity = _mapper.Map<User>(model);
                entity.Username = await EncryptionHelper.EncryptData(model.Username, userSalt);
                entity.Email = model.Email != null ? await EncryptionHelper.EncryptData(model.Email, userSalt) : "";
                entity.ContactNo = model.ContactNo != null ? await EncryptionHelper.EncryptData(model.ContactNo, userSalt) : "";
                entity.Password = await EncryptionHelper.EncryptData(model.Password, userSalt);
                entity.Salt = userSalt;
                entity.AccountStatus = UserAccountStatus.Active;

                await _userService.Insert(entity);

                // TODO: Prepare select list for User Role
                // map to user role
                await _userService.InsertMapping(entity.Id, model.UserRoleId);
            }
            return RedirectToAction("List", "Users");
        }

        public IActionResult Login()
        {
            // to always clear session whenever accessing login
            // will use this in the future for validation in accessing links
            SessionHelper.ValidateAndClearSession(HttpContext);

            // for testing
            var regions = _addressService.GetAllRegions();
            var cities = _addressService.GetAllCities();
            var barangays = _addressService.GetAllBarangays();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userService.GetUserByUsername(model.Username);
                if (user == null)
                {
                    AdminErrorNotification(model, "User doesn't exists");
                    return View(model);
                }

                var decrpytedPassword = await EncryptionHelper.DecryptData(user.Password, user.Salt);

                if (decrpytedPassword == model.Password && user.Username == model.Username)
                {
                    await SessionHelper.GenerateSession(user.Id, HttpContext);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    AdminErrorNotification(model, "Incorrect credentials entered. Please try again");
                    model.Username = null;
                    model.Password = null;
                    return RedirectToAction("Login", model);
                }
            }
            else
            {
                model.Username = null;
                model.Password = null;
                return RedirectToAction("Login", model);
            }

        }
        public IActionResult Logout()
        {
            SessionHelper.ClearSession(HttpContext);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // add permission here
            var model = await _userModelFactory.PrepareUserModel(new UserModel(), null);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserModel model)
        {
            // add permission here
            if (ModelState.IsValid)
            {
                var entity = _mapper.Map(model, new User());

                // Send SMS One Time PIN for verification
                //entity = await SendSMSHelper.SendSMSOTPRequest(entity, _smsAPIKey);
                if (_applicationSetting != null)
                {
                    if (_applicationSetting.IsEmailVerificationEnabled)
                    {
                        var generatedOTP = SendSMSHelper.GenerateOTP();
                        // Send One Time PIN in Email
                        await _emailService.SendEmailAsync(model.Email, "Email Verification",
                            $"""
                                <h2>Email Verification</h2>
                                <p>Please verify your account.</p>
                                <p>Your Generated OTP is: <b>{generatedOTP}</b></p>
                                </br>
                                <p>This OTP will be valid until: <b>{DateTime.UtcNow.AddHours(1).ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone)}</b></p>
                                </br>
                                </br>
                                <p>Please Enter the generated OTP before the expiry, to verify your account.</p>
                                </br>
                                <p>Thank You.</p>
                            """);
                        entity.OneTimePIN = generatedOTP;
                        entity.IsVerified = false;
                        entity.OneTimePINValidUntil = DateTime.UtcNow.AddHours(1);
                        entity.IsLoggedOn = false;
                    }
                    else
                    {
                        entity.IsVerified = true; 
                        entity.IsLoggedOn = true;
                    }
                }

                entity.Salt = EncryptionHelper.GenerateSalt();
                entity.Password = await EncryptionHelper.EncryptData(entity.Password, entity.Salt);
                entity.Username = await EncryptionHelper.EncryptData(entity.Username, entity.Salt);
                entity.Email = model.Email != null ? await EncryptionHelper.EncryptData(model.Email, entity.Salt) : "";
                entity.ContactNo = model.ContactNo != null ? await EncryptionHelper.EncryptData(model.ContactNo, entity.Salt) : "";

                await _userService.Insert(entity);

                // map to user role
                await _userService.InsertMapping(entity.Id, model.UserRoleId);

                if (_applicationSetting.IsEmailVerificationEnabled)
                    return RedirectToAction("VerifyAccount", new { id = entity.Id });
                else
                    return RedirectToAction("Profile", new { id = entity.Id });
            }
            else
            {
                AdminErrorNotification(model, "Failed to create user");
                return RedirectToAction("Create", model);
            }
        }

        public async Task<IActionResult> VerifyAccount(Guid id)
        {
            var entity = await _userService.GetById(id);
            if (entity == null)
            {
                // notifies error message
                return NotFound();
            }

            var model = await _userModelFactory.PrepareUserModel(new UserModel(), entity);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyAccount(UserModel model)
        {
            if (ModelState.IsValid)
            {
                var entity = await _userService.GetById(model.Id);
                if (entity == null)
                {
                    // notifies error message
                    return NotFound();
                }

                if (entity.AccountStatus != UserAccountStatus.Pending && entity.IsVerified == true)
                {
                    AdminErrorNotification(model, "User already been verified.");
                    return RedirectToAction("Profile", model);
                }

                if (string.IsNullOrEmpty(model.OneTimePIN))
                {
                    AdminErrorNotification(model, "One Time PIN is required");
                    return View(model);
                }

                if (model.OneTimePIN != entity.OneTimePIN)
                {
                    // Add configuration settings for allowable attempts for checking here
                    if (_applicationSetting != null)
                    {
                        if (entity.OneTimePINAttempt.GetValueOrDefault() > _applicationSetting.OneTimePINAttemptLimit)
                        {
                            AdminErrorNotification(model, "OTP Attempts exceeds limit");
                            return View(model);
                        }
                        else if (entity.EmailResendAttempt > _applicationSetting.EmailVerificationAttemptLimit)
                        {
                            AdminErrorNotification(model, "Email verification attempt exceeds limit");
                            return View(model);
                        }
                        else
                        {
                            entity.OneTimePINAttempt = entity.OneTimePINAttempt == null ? 1 : entity.OneTimePINAttempt += 1;
                            AdminErrorNotification(model, "Invalid One Time PIN");
                            return View(model);
                        }
                    }
                    entity.OneTimePINAttempt = entity.OneTimePINAttempt == null ? 1 : entity.OneTimePINAttempt += 1;
                    AdminErrorNotification(model, "Invalid One Time PIN");
                    return View(model);
                }

                entity.IsVerified = true;
                entity.OneTimePIN = null;
                entity.AccountStatus = UserAccountStatus.Active;
                entity.OneTimePINValidUntil = null;
                entity.OneTimePINAttempt = null;
                entity.EmailResendAttempt = null;

                await _userService.Update(entity);

                AdminSuccessNotification(model, "Successfully verified user account");
                return RedirectToAction("Profile", new { id = entity.Id });
            }
            else
            {
                AdminErrorNotification(model, "Failed to validate user");
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResendOTPEmail(Guid id)
        {
            if (id.IsNullOrEmpty())
                return NotFound();

            var entity = await _userService.GetById(id);
            if (entity == null)
            {
                // notifies error message
                return JsonError("User doesn't exists");
            }

            if (entity.AccountStatus != UserAccountStatus.Pending && entity.IsVerified == true)
            {
                return RedirectToAction("Profile", new { id = id });
            }

            if (_applicationSetting == null)
                return JsonError("Application Settings not yet configured");

            if (entity.EmailResendAttempt > _applicationSetting.EmailVerificationAttemptLimit)
            {
                return RedirectToAction("Verify Account", new { id = id });
            }

            var generatedOTP = SendSMSHelper.GenerateOTP();
            entity.OneTimePINValidUntil = DateTime.UtcNow.AddHours(1); 
            entity.OneTimePIN = generatedOTP;
            entity.EmailResendAttempt = entity.EmailResendAttempt.GetValueOrDefault() + 1;
            // Send One Time PIN in Email
            await _emailService.SendEmailAsync(await EncryptionHelper.DecryptData(entity.Email, entity.Salt), "Email Verification", 
                $"""
                <h2>Email Verification</h2>
                <p>Please verify your account.</p>
                <p>Your Generated OTP is: <b>{generatedOTP}</b></p>
                </br>
                <p>This OTP will be valid until: <b>{entity.OneTimePINValidUntil.GetValueOrDefault().ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone)}</b></p>
                </br>
                </br>
                <p>Please Enter the generated OTP before the expiry, to verify your account.</p>
                </br>
                <p>Thank You.</p>
                """);

            await _userService.Update(entity);
            return RedirectToAction("VerifyAccount", new { id = id });
        }

        public async Task<IActionResult> Profile(Guid id)
        {
            var entity = await _userService.GetById(id);
            if (entity == null)
            {
                // notifies error message
                return NotFound();
            }

            var model = await _userModelFactory.PrepareUserModel(new UserModel(), entity);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(UserModel model)
        {
            var entity = await _userService.GetById(model.Id);
            if (entity == null)
            {
                // notifies error message
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // before updating user and password and account status are null
                // for now do it like this 
                // but for later will add it in prepare model
                var password = entity.Password;
                var accountStatus = entity.AccountStatus;

                entity = _mapper.Map(model, entity);

                entity.ModifiedOn = DateTime.UtcNow;
                entity.Password = password;
                entity.AccountStatus = accountStatus;
                // need to check if user change role
                var userRoleMappedToUser = await _userService.GetUserRoleMappingByUserId(model.Id);
                if (userRoleMappedToUser != null)
                {
                    // means not the same user role
                    // need to update user role mapping
                    if (userRoleMappedToUser.UserRoleId != model.UserRoleId)
                    {
                        userRoleMappedToUser.UserRoleId = model.UserRoleId;
                        await _userService.UpdateMapping(userRoleMappedToUser);
                    }
                }
                else
                {
                    await _userService.InsertMapping(model.Id, model.UserRoleId);
                }

                await _userService.Update(entity);

                model.Password = AdminMessages.HiddenPasswordDisplay;

                AdminSuccessNotification(model, "Successfully updated User");
            }
            else
            {
                AdminErrorNotification(model, "Failed to update User");
            }

            return RedirectToAction("Profile", model);
        }

        public async Task<IActionResult> ChangePassword(Guid id)
        {
            if (id.IsNullOrEmpty())
                return NotFound();

            var entity = await _userService.GetById(id);
            if (entity == null)
                return NotFound();

            var model = await _userModelFactory.PrepareUserModel(new UserModel(), entity);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(UserModel model)
        {
            if (model.Id.IsNullOrEmpty())
                return NotFound();

            var entity = await _userService.GetById(model.Id);
            if (entity == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                bool hasError = false;
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    hasError = true;
                    AdminErrorNotification(model, "Current password is required");
                }
                else if (string.IsNullOrEmpty(model.NewPassword))
                {
                    hasError = true;
                    AdminErrorNotification(model, "New password is required");
                }
                else if (string.IsNullOrEmpty(model.ConfirmNewPassword))
                {
                    hasError = true;
                    AdminErrorNotification(model, "Confirm new password is required");
                }
                else if (entity.Password != model.CurrentPassword)
                {
                    hasError = true;
                    AdminErrorNotification(model, "Current password is incorrect");
                }
                else if (entity.Password == model.NewPassword)
                {
                    hasError = true;
                    AdminErrorNotification(model, "New password must be different from current password");
                }
                else if (model.NewPassword != model.ConfirmNewPassword)
                {
                    hasError = true;
                    AdminErrorNotification(model, "New password and confirm new password do not match");
                }

                if (!hasError)
                {
                    entity.Password = await EncryptionHelper.EncryptData(model.NewPassword, entity.Salt);
                    entity.ModifiedOn = DateTime.UtcNow;
                    entity.LastPasswordChangedDate = DateTime.UtcNow;
                    await _userService.Update(entity);
                    AdminSuccessNotification(model, "Successfully updated password");

                    return RedirectToAction("Profile", model);
                }

                return RedirectToAction("ChangePassword", model);
            }
            else
            {
                AdminErrorNotification(model, "Failed to update password");
                return RedirectToAction("ChangePassword", model);
            }
        }

        #endregion

        #region User Roles CRUD

        public async Task<IActionResult> UserRoleList(int page = 1)
        {
            var model = await _userModelFactory.PrepareUserRoleSearchModelAsync(new UserRoleSearchModel(), page, 10);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> UserRoleListSearch(UserRoleSearchModel searchModel)
        {
            var model = await _userModelFactory.PrepareUserRoleListModelAsync(searchModel);

            return PartialView("_UserRoleSearchList", model);
        }

        public async Task<IActionResult> CreateRole()
        {
            // add permission here if user role can create
            // add prepare model in create
            var model = await _userModelFactory.PrepareUserRoleModelAsync(new UserRoleModel(), null);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(UserRoleModel model)
        {
            // add permission here if user role can create
            // add prepare model in create
            if (ModelState.IsValid)
            {
                var entity = new UserRole()
                {
                    Rolename = model.Rolename
                };
                await _userService.InsertRole(entity);
            }
            else
            {
                return JsonError(ModelJsonValidationErrorMessages(ModelState));
            }

            return NullJsonResult();
        }

        public async Task<IActionResult> UserRole(Guid id)
        {
            var entity = await _userService.GetUserRoleById(id);
            if (entity == null)
            {
                // notification error message
                return NotFound();
            }

            var model = await _userModelFactory.PrepareUserRoleModelAsync(new UserRoleModel(), entity);

            return View(model);
        }

        #endregion
    }
}
