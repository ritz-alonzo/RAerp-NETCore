using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RA.Core.Models.BaseModels;
using RAerp.Data;
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
            BaseNotificationModelFactory<BaseModel>.PrepareNotificationModel(model, NotificationStatus.Success, message);

            TempData["notificationScript"] = model.JSNotificationFunction;
            TempData["notificationMessage"] = model.NotificationMessage;
        }
        /// <summary>
        /// Notifies user for error action
        /// </summary>
        public void ErrorNotification(BaseModel model, string message)
        {
            BaseNotificationModelFactory<BaseModel>.PrepareNotificationModel(model, NotificationStatus.Error, message);

            TempData["notificationScript"] = model.JSNotificationFunction;
            TempData["notificationMessage"] = model.NotificationMessage;
        }
        /// <summary>
        /// Notifies user for error action with exception
        /// </summary>
        public void ErrorNotification(BaseModel model, Exception exception)
        {
            BaseNotificationModelFactory<BaseModel>.PrepareNotificationModel(model, NotificationStatus.Error, exception.Message);

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
    }
}
