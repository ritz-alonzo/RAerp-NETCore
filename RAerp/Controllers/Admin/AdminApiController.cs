using Microsoft.AspNetCore.Mvc;
using RA.Core.Domain;
using RA.Data.Domain.Users;
using RAerp.Helpers.UserHelper;
using RAerp.Models.ApiModel;
using RAerp.Security.AccessRightsControl;
using RAerp.Services.UserServices;
using System.Net;

namespace RAerp.Controllers.Admin
{
    public class AdminApiController : ControllerBase
    {
        #region Methods
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

        public ApiResponseErrorModel GenerateErrorResponseModel(HttpStatusCode statusCode, string message)
        {
            var errorResponseModel = new ApiResponseErrorModel();
            errorResponseModel.Message = message;
            errorResponseModel.Success = false;
            switch (statusCode)
            {
                case HttpStatusCode.OK:
                    errorResponseModel.Status = HttpStatusCode.OK.ToString();
                    break;

                case HttpStatusCode.NotFound:
                    errorResponseModel.Status = HttpStatusCode.NotFound.ToString();
                    break;

                case HttpStatusCode.Unauthorized:
                    errorResponseModel.Status = HttpStatusCode.Unauthorized.ToString();
                    break;
            }
            return errorResponseModel;
        }

        public async Task<IActionResult> ValidateUserAccessAndCredentials<TForm>(IAccessControl accessControl, IUserIdentity userIdentity)
            where TForm : BaseEntity
        {
            // Access rights
            if (!accessControl.HasViewAccessAsync<TForm>().Result)
                return Unauthorized();

            // Validate User if logged in
            var currentUser = await GetCurrentCredentialsAsync(userIdentity);
            if (currentUser == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "User has not yet logged in"));

            return Ok();
        }

        public async Task<User> GetCurrentCredentialsAsync(IUserIdentity userIdentity)
        {
            return await userIdentity.GetCurrentApiUserAsync(HttpContext.User);
        }

        public async Task ValidateRequestEntityValues()
        {
            
        }
        #endregion
    }
}
