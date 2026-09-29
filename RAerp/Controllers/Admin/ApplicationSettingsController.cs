using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RAerp.Domain.Application;
using RAerp.Factories.ApplicationSettingFactory;
using RAerp.Helpers.Security;
using RAerp.Helpers.UserHelper;
using RAerp.Models.ApplicationSettingsModel;
using RAerp.Services.ApplicationSettingServices;
using RAerp.Services.UserServices;

namespace RAerp.Controllers.Admin
{
    public class ApplicationSettingsController : AdminController
    {
        private readonly IApplicationSettingService _applicationSettingService;
        private readonly IApplicationSettingModelFactory _applicationSettingModelFactory;
        private readonly IMapper _mapper;
        private readonly IUserIdentity _userIdentity;

        public ApplicationSettingsController(
            IApplicationSettingService applicationSettingService,
            IApplicationSettingModelFactory applicationSettingModelFactory,
            IMapper mapper,
            IUserIdentity userIdentity)
        {
            _applicationSettingService = applicationSettingService;
            _applicationSettingModelFactory = applicationSettingModelFactory;
            _mapper = mapper;
            _userIdentity = userIdentity;
        }

        #region Methods

        // GET: ApplicationSettings/Create/Test
        public async Task<IActionResult> Create()
        {
            var model = await _applicationSettingModelFactory.PrepareApplicationSettingModelAsync();
            return View("~/Views/ApplicationSettings/Create.cshtml", model);
        }

        // POST: ApplicationSettings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationSettingModel model)
        {
            if (!ModelState.IsValid)
            {
                AdminErrorNotification(model, "Please fix the validation errors.");
                return View(model);
            }

            try
            {
                var applicationSetting = _mapper.Map<ApplicationSetting>(model);
                
                // Set audit fields
                var currentUser = await _userIdentity.GetCurrentUserAsync(HttpContext);
                applicationSetting.CreatedById = currentUser.Id;
                applicationSetting.ModifiedById = currentUser.Id;

                await _applicationSettingService.InsertAsync(applicationSetting);

                AdminSuccessNotification(model, "Application setting created successfully.");
                return RedirectToAction(nameof(Index), new { id = applicationSetting.Id });
            }
            catch (Exception ex)
            {
                AdminErrorNotification(model, $"Error creating application setting: {ex.Message}");
                return View(model);
            }
        }

        // GET: ApplicationSettings/Edit/5
        public async Task<IActionResult> Index()
        {
            var applicationSetting = _applicationSettingService.GetListAsync().Result.FirstOrDefault();

            var model = await _applicationSettingModelFactory.PrepareApplicationSettingModelAsync(null, applicationSetting == null ? null : applicationSetting?.Id);
            return View(model);
        }

        // POST: ApplicationSettings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplicationSettingModel model)
        {
            if (!ModelState.IsValid)
            {
                AdminErrorNotification(model, "Please fix the validation errors.");
                return View(nameof(Index), model);
            }

            try
            {
                var applicationSetting = await _applicationSettingService.GetByIdAsync(model.Id);
                if (applicationSetting == null)
                {
                    AdminErrorNotification(model, "Application setting not found.");
                    return RedirectToAction("Index", "Home");
                }

                // Map model to entity
                _mapper.Map(model, applicationSetting);

                // Set audit fields
                var currentUser = await _userIdentity.GetCurrentUserAsync(HttpContext);
                applicationSetting.ModifiedById = currentUser.Id;

                await _applicationSettingService.UpdateAsync(applicationSetting);

                AdminSuccessNotification(model, "Application setting updated successfully.");
                return RedirectToAction(nameof(Index), new { id = model.Id });
            }
            catch (Exception ex)
            {
                AdminErrorNotification(model, $"Error updating application setting: {ex.Message}");
                return RedirectToAction(nameof(Index), model);
            }
        }

        // POST: ApplicationSettings/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var applicationSetting = await _applicationSettingService.GetByIdAsync(id);
                if (applicationSetting == null)
                    return JsonError("Application setting not found.");

                await _applicationSettingService.DeleteAsync(applicationSetting);

                return NullJsonResult();
            }
            catch (Exception ex)
            {
                return JsonError($"Error deleting application setting: {ex.Message}");
            }
        }

        // POST: ApplicationSettings/GenerateClientId/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateClientId(Guid id)
        {
            try
            {
                var applicationSetting = await _applicationSettingService.GetByIdAsync(id);
                if (applicationSetting == null)
                    return JsonError("Application setting not found.");

                // Regenerate client ID
                applicationSetting.ClientId = EncryptionHelper.GenerateSalt();

                // Set audit fields
                var currentUser = await _userIdentity.GetCurrentUserAsync(HttpContext);
                applicationSetting.ModifiedById = currentUser.Id;

                await _applicationSettingService.UpdateAsync(applicationSetting);

                return Json(new { success = true, clientId = applicationSetting.ClientId });
            }
            catch (Exception ex)
            {
                return JsonError($"Error regenerating client ID: {ex.Message}");
            }
        }

        // POST: ApplicationSettings/RegenerateSecret/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegenerateSecret(Guid id)
        {
            try
            {
                var applicationSetting = await _applicationSettingService.GetByIdAsync(id);
                if (applicationSetting == null)
                    return JsonError("Application setting not found.");

                // Regenerate client secret
                applicationSetting.ClientSecret = EncryptionHelper.GenerateSalt();
                
                // Set audit fields
                var currentUser = await _userIdentity.GetCurrentUserAsync(HttpContext);
                applicationSetting.ModifiedById = currentUser.Id;

                await _applicationSettingService.UpdateAsync(applicationSetting);

                return Json(new { success = true, clientSecret = applicationSetting.ClientSecret });
            }
            catch (Exception ex)
            {
                return JsonError($"Error regenerating secret: {ex.Message}");
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Generates a secure random client secret
        /// </summary>
        private string GenerateClientSecret()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            var secret = new string(Enumerable.Repeat(chars, 64)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(secret));
        }

        #endregion
    }
}
