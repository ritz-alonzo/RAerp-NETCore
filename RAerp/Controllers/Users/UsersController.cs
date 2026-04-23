#region namespaces
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RA.Data.App_Data;
using RA.Data.Data;
using RA.Data.Domain.Users;
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
using RAerp.Services.UserServices;
using System.Reflection;
#endregion

namespace RAErp.Controllers.Users
{
    [AutoValidateAntiforgeryToken]
    public class UsersController : AdminController
    {
        private readonly RAerpContext _erpContext;
        private readonly IUserService _userService;
        private readonly IUserModelFactory _userModelFactory;
        private readonly IMapper _mapper;
        private readonly IAccessControl _accessControl;
        private readonly IAddressService _addressService;
        private readonly string _smsAPIKey;

        public UsersController(RAerpContext erpContext,
            IUserService userService,
            IUserModelFactory userModelFactory,
            IMapper mapper,
            IAccessControl accessControl,
            IAddressService addressService,
            IConfiguration configuration)
        {
            _erpContext = erpContext;
            _userService = userService;
            _userModelFactory = userModelFactory;
            _mapper = mapper;
            _accessControl = accessControl;
            _addressService = addressService;
            _smsAPIKey = configuration.GetValue<string>("ApiSettings:SMSApiKey");
        }

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
                    return View(model);
                }
            }
            else
            {
                return View(model);
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
                entity = await SendSMSHelper.SendSMSOTPRequest(entity, _smsAPIKey);

                entity.Salt = EncryptionHelper.GenerateSalt();
                entity.Password = await EncryptionHelper.EncryptData(entity.Password, entity.Salt);
                entity.Email = await EncryptionHelper.EncryptData(entity.Email, entity.Salt);
                entity.ContactNo = await EncryptionHelper.EncryptData(entity.ContactNo, entity.Salt);

                await _userService.Insert(entity);

                return RedirectToAction("VerifyAccount", new { id = entity.Id });
            }
            else
            {
                AdminErrorNotification(model, "Failed to create user");
                return View(model);
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

                if (string.IsNullOrEmpty(model.OneTimePIN))
                {
                    AdminErrorNotification(model, "One Time PIN is required");
                    return View(model);
                }

                if (model.OneTimePIN != entity.OneTimePIN)
                {
                    AdminErrorNotification(model, "Invalid One Time PIN");
                    return View(model);
                }

                entity.IsVerified = true;
                entity.OneTimePIN = null;
                entity.AccountStatus = UserAccountStatus.Active;

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

                entity.ModifiedOn = DateTime.Now;
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

            return View(model);
        }

        // will add functionality here
        public IActionResult ChangePassword()
        {
            return View();
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
