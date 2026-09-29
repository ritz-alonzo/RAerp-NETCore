using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RA.Core.Domain;
using RA.Data.Data;
using RAerp.Domain.Application;
using RAerp.Domain.Users;
using RAerp.Extensions;
using RAerp.Helpers.UserHelper;
using RAerp.Models.ApiModel;
using RAerp.Security.AccessRightsControl;
using RAerp.Services.ApplicationSettingServices;
using RAerp.Services.UserServices;
using System.Net;

namespace RAerp.Controllers.Admin
{
    public class AdminApiController : ControllerBase
    {
        #region Methods
        [NonAction]
        public ApiResponseListModel<TEntity> GenerateListResponseModel<TEntity>(HttpStatusCode statusCode, string message, int? pageNumber = null, int? pageSize = null, IEnumerable<TEntity> dataList = null)
            where TEntity : BaseEntity
        {
            var responseModel = new ApiResponseListModel<TEntity>();
            responseModel.Message = message;
            switch (statusCode)
            {
                case HttpStatusCode.OK:
                    responseModel.Status = HttpStatusCode.OK.ToString();
                    responseModel.Success = true;
                    break;

                case HttpStatusCode.NotFound:
                    responseModel.Status = HttpStatusCode.NotFound.ToString();
                    responseModel.Success = false;
                    break;

                case HttpStatusCode.Unauthorized:
                    responseModel.Status = HttpStatusCode.Unauthorized.ToString();
                    responseModel.Success = false;
                    break;
            }

            if (dataList.Any() && dataList != null)
            {
                List<TEntity> list = dataList.ToList();
                responseModel.Data = list;
                responseModel.TotalCount = list.Count;
                responseModel.PageNumber = pageNumber ?? 1;
                responseModel.PageSize = pageSize ?? int.MaxValue;
            }

            return responseModel;
        }

        [NonAction]
        public ApiResponseModel<TEntity> GenerateResponseModel<TEntity>(HttpStatusCode statusCode, string message, TEntity data = null)
            where TEntity : BaseEntity
        {
            var responseModel = new ApiResponseModel<TEntity>();
            responseModel.Message = message;
            switch (statusCode)
            {
                case HttpStatusCode.OK:
                    responseModel.Status = HttpStatusCode.OK.ToString();
                    responseModel.Success = true;
                    break;

                case HttpStatusCode.Created:
                    responseModel.Status = HttpStatusCode.Created.ToString();
                    responseModel.Success = true;
                    break;

                case HttpStatusCode.NotFound:
                    responseModel.Status = HttpStatusCode.NotFound.ToString();
                    responseModel.Success = false;
                    break;

                case HttpStatusCode.Unauthorized:
                    responseModel.Status = HttpStatusCode.Unauthorized.ToString();
                    responseModel.Success = false;
                    break;

                case HttpStatusCode.BadRequest:
                    responseModel.Status = HttpStatusCode.BadRequest.ToString();
                    responseModel.Success = false;
                    break;
            }

            if (data != null)
            {
                responseModel.Data = data;
            }

            return responseModel;
        }
        [NonAction]
        public ApiResponseAdminListModel<TEntity> GenerateListResponseAdminModel<TEntity>(HttpStatusCode statusCode, string message, IEnumerable<TEntity> dataList = null)
            where TEntity : BaseAdminEntity
        {
            var responseModel = new ApiResponseAdminListModel<TEntity>();
            responseModel.Message = message;
            switch (statusCode)
            {
                case HttpStatusCode.OK:
                    responseModel.Status = HttpStatusCode.OK.ToString();
                    responseModel.Success = true;
                    break;

                case HttpStatusCode.NotFound:
                    responseModel.Status = HttpStatusCode.NotFound.ToString();
                    responseModel.Success = false;
                    break;

                case HttpStatusCode.Unauthorized:
                    responseModel.Status = HttpStatusCode.Unauthorized.ToString();
                    responseModel.Success = false;
                    break;
            }

            if (dataList.Any() && dataList != null)
            {
                responseModel.Data = dataList.ToList();
            }

            return responseModel;
        }
        [NonAction]
        public ApiResponseAdminModel<TEntity> GenerateResponseAdminModel<TEntity>(HttpStatusCode statusCode, string message, TEntity data = null)
            where TEntity : BaseAdminEntity
        {
            var responseModel = new ApiResponseAdminModel<TEntity>();
            responseModel.Message = message;
            switch (statusCode)
            {
                case HttpStatusCode.OK:
                    responseModel.Status = HttpStatusCode.OK.ToString();
                    responseModel.Success = true;
                    break;

                case HttpStatusCode.NotFound:
                    responseModel.Status = HttpStatusCode.NotFound.ToString();
                    responseModel.Success = false;
                    break;

                case HttpStatusCode.Unauthorized:
                    responseModel.Status = HttpStatusCode.Unauthorized.ToString();
                    responseModel.Success = false;
                    break;
            }

            if (data != null)
            {
                responseModel.Data = data;
            }

            return responseModel;
        }

        [NonAction]
        public ApiResponseErrorModel GenerateErrorResponseModel(HttpStatusCode statusCode, string message)
        {
            var errorResponseModel = new ApiResponseErrorModel();
            errorResponseModel.Message = message;
            errorResponseModel.Success = false;
            errorResponseModel.Status = statusCode.ToString();
            //switch (statusCode)
            //{
            //    case HttpStatusCode.OK:
            //        errorResponseModel.Status = HttpStatusCode.OK.ToString();
            //        break;

            //    case HttpStatusCode.NotFound:
            //        errorResponseModel.Status = HttpStatusCode.NotFound.ToString();
            //        break;

            //    case HttpStatusCode.Unauthorized:
            //        errorResponseModel.Status = HttpStatusCode.Unauthorized.ToString();
            //        break;
            //}
            return errorResponseModel;
        }
        [NonAction]
        public async Task<ApiValidationModel> ValidateUserAccessAndCredentials<TForm>(IAccessControl accessControl, IUserIdentity userIdentity, ApplicationSetting applicationSetting)
            where TForm : BaseEntity
        {
            ApiValidationModel model = new ApiValidationModel();
            // Access rights
            if (!accessControl.HasViewAccessAsync<TForm>().Result)
            {
                model.IsPassed = false;
                model.StatusCode = HttpStatusCode.Forbidden;
                model.Message = "You do not have access rights to view this resource.";
                return model;
            }

            // Validate User if logged in
            var currentUser = await GetCurrentCredentialsAsync(userIdentity);
            if (currentUser == null)
            {
                model.IsPassed = false;
                model.StatusCode = HttpStatusCode.NotFound;
                model.Message = "User has not yet logged in";
                return model;
            }

            if (currentUser.AccountStatus != UserAccountStatus.Active)
            {
                model.IsPassed = false;
                model.StatusCode = HttpStatusCode.Forbidden;
                model.Message = "User account is not active.";
                return model;
            }

            if (currentUser.IsVerified != true)
            {
                model.IsPassed = false;
                model.StatusCode = HttpStatusCode.Forbidden;
                model.Message = "User account is not verified.";
                return model;
            }

            string clientId = HttpContext.Request.Cookies["client_id"];
            string clientSecret = HttpContext.Request.Cookies["client_secret"];

            if (string.IsNullOrEmpty(clientId))
            {
                model.IsPassed = false;
                model.StatusCode = HttpStatusCode.NotFound;
                model.Message = "No client id in Headers.";
                return model;
            }

            if (string.IsNullOrEmpty(clientSecret))
            {
                model.IsPassed = false;
                model.StatusCode = HttpStatusCode.NotFound;
                model.Message = "No client secret in Headers.";
                return model;
            }

            if (clientId != applicationSetting.ClientId || clientSecret != applicationSetting.ClientSecret)
            {
                model.IsPassed = false;
                model.StatusCode = HttpStatusCode.Forbidden;
                model.Message = "Invalid client id or client secret.";
                return model;
            }

            return model;
        }
        [NonAction]
        public async Task<User> GetCurrentCredentialsAsync(IUserIdentity userIdentity)
        {
            return await userIdentity.GetCurrentApiUserAsync(HttpContext.User);
        }
        [NonAction]
        public async Task<ApiRequestValidationModel> ValidateRequestEntityValues<TRequest, TRequestValidator>(TRequest requestDto)
            where TRequest : BaseEntity
            where TRequestValidator : AbstractValidator<TRequest>
        {
            ApiRequestValidationModel apiRequestValidationModel = new ApiRequestValidationModel();

            var validator = (IValidator<TRequest>)Activator.CreateInstance(typeof(TRequestValidator));
            if (validator != null)
            {
                var validationResult = await validator.ValidateAsync(requestDto);
                if (!validationResult.IsValid)
                {
                    apiRequestValidationModel.IsValid = false;
                    apiRequestValidationModel.ValidationMessage = validationResult.RequestDtoValidationErrors("Validation failed:");
                }
            }

            return apiRequestValidationModel;
        }
        #endregion
    }
}
