using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RA.Core.Models.BaseModels;
using RAerp.Data;
using RAerp.Extensions;
using RAerp.Factories.CoreFactories;
using System;
using System.Net;

namespace RAerp.Controllers.Admin
{
    public class AdminController : Controller
    {
        #region Admin Notifications
        /// <summary>
        /// Notifies user for success action
        /// </summary>
        public void AdminSuccessNotification(BaseAdminModel model, string message)
        {
            BaseAdminNotificationModelFactory<BaseAdminModel>.PrepareNotificationModel(model, NotificationStatus.Success, message);

            TempData["notificationScript"] = model.JSNotificationFunction;
            TempData["notificationMessage"] = model.NotificationMessage;
        }
        /// <summary>
        /// Notifies user for error action
        /// </summary>
        public void AdminErrorNotification(BaseAdminModel model, string message)
        {
            BaseAdminNotificationModelFactory<BaseAdminModel>.PrepareNotificationModel(model, NotificationStatus.Error, message);

            TempData["notificationScript"] = model.JSNotificationFunction;
            TempData["notificationMessage"] = model.NotificationMessage;
        }
        /// <summary>
        /// Notifies user for error action with exception
        /// </summary>
        public void AdminErrorNotification(BaseAdminModel model, Exception exception)
        {
            BaseAdminNotificationModelFactory<BaseAdminModel>.PrepareNotificationModel(model, NotificationStatus.Error, exception.Message);

            TempData["notificationScript"] = model.JSNotificationFunction;
            TempData["notificationMessage"] = model.NotificationMessage;
        }
        #endregion

        /// <summary>
        /// Notifies user for success action
        /// </summary>
        public void SuccessNotification(BaseModel model, string message)
        {
            BaseNotificationModelFactory<BaseModel>.PrepareNotificationModel(model, NotificationStatus.Success, message, ModelState);

            TempData["notificationScript"] = model.JSNotificationFunction;
            TempData["notificationMessage"] = model.NotificationMessage;
        }
        /// <summary>
        /// Notifies user for error action
        /// </summary>
        public void ErrorNotification(BaseModel model, string message)
        {
            BaseNotificationModelFactory<BaseModel>.PrepareNotificationModel(model, NotificationStatus.Error, message, ModelState);

            TempData["notificationScript"] = model.JSNotificationFunction;
            TempData["notificationMessage"] = model.NotificationMessage;
        }

        public void ErrorNotification<TModel, TValidator>(TModel model, string message)
            where TModel : BaseModel
            where TValidator : AbstractValidator<TModel>
        {
            BaseNotificationModelFactory<BaseModel>.PrepareNotificationModel(model, NotificationStatus.Error, message, ModelState);

            var validator = (IValidator<TModel>)Activator.CreateInstance(typeof(TValidator));
            if (validator != null)
            {
                var validationResult = validator.Validate(model);
                if (!validationResult.IsValid)
                {
                    model.NotificationMessage = validationResult.AddToModelState(ModelState, message);
                }
            }
            TempData["notificationScript"] = model.JSNotificationFunction;
            TempData["notificationMessage"] = model.NotificationMessage;
        }
        /// <summary>
        /// Notifies user for error action with exception
        /// </summary>
        public void ErrorNotification(BaseModel model, Exception exception)
        {
            BaseNotificationModelFactory<BaseModel>.PrepareNotificationModel(model, NotificationStatus.Error, exception.Message, ModelState);

            TempData["notificationScript"] = model.JSNotificationFunction;
            TempData["notificationMessage"] = model.NotificationMessage;
        }
        /// <summary>
        /// returns Json with error message
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public IActionResult JsonError(string errorMessage)
        {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json(new
            {
                Error = errorMessage
            });
        }

        public IActionResult NullJsonResult()
        {
            return Json(null);
        }

        public string ModelJsonValidationErrorMessages(ModelStateDictionary modelState)
        {
            var invalidModelStates = modelState.Select(c => c.Value).Where(c => c.ValidationState == ModelValidationState.Invalid).ToList();
            if (!invalidModelStates.Any())
                return "";

            var errorMessage = "";

            foreach (var invalidState in invalidModelStates)
            {
                var invalidStatErrorMessage = invalidState.Errors.First().ErrorMessage.ToString();

                if (string.IsNullOrEmpty(errorMessage))
                    errorMessage = invalidStatErrorMessage;
                else
                    errorMessage = errorMessage + ", " + invalidStatErrorMessage;
            }

            return errorMessage;
        }

        public IActionResult UnauthorizedAccess()
        {
            return RedirectToAction("AccessDenied", "AccessRights");
        }



        [HttpGet]
        public IActionResult DownloadFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return NotFound();

            IWebHostEnvironment env = HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment)) as IWebHostEnvironment;

            var fullPath = Path.Combine(env.WebRootPath, fileName.TrimStart('/'));

            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);

            return File(stream, "application/octet-stream", Path.GetFileName(fullPath));
        }
    }
}
