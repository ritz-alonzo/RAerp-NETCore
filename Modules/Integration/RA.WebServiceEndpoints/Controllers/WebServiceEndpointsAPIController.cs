using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RA.BusinessEntities.Data;
using RA.BusinessEntities.Domain;
using RA.BusinessEntities.Services;
using RA.Categories.Data;
using RA.Categories.Domain;
using RA.Categories.Services;
using RA.Core.Domain;
using RA.Data.Domain.Users;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;
using RA.WebServiceEndpoints.Models;
using RA.WebServiceEndpoints.Services;
using RAerp.Services.UserServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
// 04-21-25 to have:
// 1. Need to add JWT Token generation and validation - done (04-23-25)
// 2. Need to add Access Rights
namespace RA.WebServiceEndpoints.Controllers
{
    [ApiController]
    [Route("api/[controller]/{endpoint}")]
    public class WebServiceEndpointsAPIController : ControllerBase
    {
        private readonly IWebServiceEndpointService _webServiceEndpointService;
        private readonly IEntityTypeManager _entityTypeManager;
        private readonly IBusinessEntityService _businessEntityService;
        private readonly IUserService _userService;
        private readonly ICategoryService _categoryService;

        public WebServiceEndpointsAPIController(IWebServiceEndpointService webServiceEndpointService,
            IEntityTypeManager entityTypeManager,
            IBusinessEntityService businessEntityService,
            IUserService userService,
            ICategoryService categoryService)
        {
            _webServiceEndpointService = webServiceEndpointService;
            _entityTypeManager = entityTypeManager;
            _businessEntityService = businessEntityService;
            _userService = userService;
            _categoryService = categoryService;
        }

        #region CRUD

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetList(string endpoint)
        {
            // TODO: will add checking of access rights here
            //

            if (string.IsNullOrEmpty(endpoint))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint cannot be empty"));

            var webServiceEndpoint = await _webServiceEndpointService.GetEndpointByEndpointName(endpoint);
            if (webServiceEndpoint == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint doesn't exists, create this endpoint in Web Service Endpoints screen"));

            var currentUser = await GetCurrentUser();
            if (currentUser == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "User has not yet logged in"));

            #region Switch Domain

            switch (webServiceEndpoint.EndpointDomain)
            {
                case "BusinessEntity":
                    var businessEntityList = await _businessEntityService.GetList(webServiceEndpoint.EndpointEntityTypeId.Value);
                    if (businessEntityList.Any())
                        return Ok(GenerateEntityListResponseModel(HttpStatusCode.OK, "Successful GET", businessEntityList));
                    break;

                case "Category":
                    var categoryEntityList = await _categoryService.GetList(webServiceEndpoint.EndpointEntityTypeId.Value);
                    if (categoryEntityList.Any())
                        return Ok(GenerateEntityListResponseModel(HttpStatusCode.OK, "Successful GET", categoryEntityList));
                    break;
            }
            #endregion

            return Ok();
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(string endpoint, string id)
        {
            // TODO: will add checking of access rights here
            //

            if (string.IsNullOrEmpty(endpoint))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint cannot be empty"));

            var webServiceEndpoint = await _webServiceEndpointService.GetEndpointByEndpointName(endpoint);
            if (webServiceEndpoint == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint doesn't exists, create this endpoint in Web Service Endpoints screen"));

            var currentUser = await GetCurrentUser();
            if (currentUser == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "User has not yet logged in"));

            if (string.IsNullOrEmpty(id))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Id cannot be found"));

            var entityId = Guid.Parse(id);
            if (entityId.IsNullOrEmpty())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Id cannot be empty"));

            #region Switch Domain

            switch (webServiceEndpoint.EndpointDomain)
            {
                case "BusinessEntity":
                    var businessEntity = await _businessEntityService.GetById(entityId);
                    if (businessEntity != null)
                        return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successful GET", businessEntity));
                    break;

                case "Category":
                    var categoryEntity = await _categoryService.GetById(entityId);
                    if (categoryEntity != null)
                        return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successful GET", categoryEntity));
                    break;
            }
            #endregion

            return Ok();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(string endpoint, [FromBody] Dictionary<string, object> data)
        {
            // TODO: will add checking of access rights here
            //

            if (string.IsNullOrEmpty(endpoint))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint cannot be empty"));

            var webServiceEndpoint = await _webServiceEndpointService.GetEndpointByEndpointName(endpoint);
            if (webServiceEndpoint == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint doesn't exists, create this endpoint in Web Service Endpoints screen"));

            var currentUser = await GetCurrentUser();
            if (currentUser == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "User has not yet logged in"));

            if (data == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Request body cannot be empty."));

            #region Switch Domain

            switch (webServiceEndpoint.EndpointDomain)
            {
                case "BusinessEntity":
                    var businessEntityRequest = (BusinessEntity)CreateEntityFromDictionary(typeof(BusinessEntity), data);
                    if (businessEntityRequest == null)
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Incorrect object mapping"));

                    var businessEntitySettings = await _entityTypeManager.GetSettingDataOfEntity<BusinessEntity, BusinessEntitySetting>(businessEntityRequest.EntityTypeId);
                    if (businessEntitySettings != null)
                    {
                        if (businessEntitySettings.AutoGeneratedTemplate)
                            businessEntityRequest.Code = null;
                    }

                    businessEntityRequest.CreatedById = currentUser.Id;
                    businessEntityRequest.ModifiedOn = null;
                    businessEntityRequest.ModifiedById = null;

                    await _businessEntityService.Insert(businessEntityRequest);
                    return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successfully created Business Entity", businessEntityRequest));

                case "Category":
                    var categoryEntityRequest = (Category)CreateEntityFromDictionary(typeof(Category), data);
                    if (categoryEntityRequest == null)
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Incorrect object mapping"));

                    var categoryEntitySettings = await _entityTypeManager.GetSettingDataOfEntity<Category, CategorySetting>(categoryEntityRequest.EntityTypeId);
                    if (categoryEntitySettings != null)
                    {
                        if (categoryEntitySettings.AutoGeneratedTemplate)
                            categoryEntityRequest.Code = null;
                    }

                    categoryEntityRequest.CreatedById = currentUser.Id;
                    categoryEntityRequest.ModifiedOn = null;
                    categoryEntityRequest.ModifiedById = null;

                    await _categoryService.Insert(categoryEntityRequest);
                    return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successfully created Category", categoryEntityRequest));
            }
            #endregion

            return Ok();
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Update(string endpoint, [FromBody] Dictionary<string, object> data)
        {
            // TODO: will add checking of access rights here
            //

            if (string.IsNullOrEmpty(endpoint))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint cannot be empty"));

            var webServiceEndpoint = await _webServiceEndpointService.GetEndpointByEndpointName(endpoint);
            if (webServiceEndpoint == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint doesn't exists, create this endpoint in Web Service Endpoints screen"));

            var currentUser = await GetCurrentUser();
            if (currentUser == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "User has not yet logged in"));

            if (data == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Request body cannot be empty."));

            #region Switch Domain

            switch (webServiceEndpoint.EndpointDomain)
            {
                case "BusinessEntity":
                    var businessEntityRequest = (BusinessEntity)CreateEntityFromDictionary(typeof(BusinessEntity), data);
                    if (businessEntityRequest == null)
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Incorrect object mapping"));

                    if (businessEntityRequest.Id.IsNullOrEmpty())
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Business Entity cannot be found"));

                    var businessEntity = await _businessEntityService.GetById(businessEntityRequest.Id);
                    businessEntity.Name = businessEntity.Name != businessEntityRequest.Name ? businessEntityRequest.Name : businessEntity.Name;
                    businessEntity.Description = businessEntity.Description != businessEntityRequest.Description ? businessEntityRequest.Description : businessEntity.Description;
                    businessEntity.StatusId = businessEntity.StatusId != businessEntityRequest.StatusId ? businessEntityRequest.StatusId : businessEntity.StatusId;
                    businessEntity.CategoryId = businessEntity.CategoryId != businessEntityRequest.CategoryId ? businessEntityRequest.CategoryId : businessEntity.CategoryId;
                    businessEntityRequest.ModifiedById = currentUser.Id;

                    await _businessEntityService.Update(businessEntityRequest);
                    return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successfully updated Business Entity", businessEntityRequest));

                case "Category":
                    var categoryEntityRequest = (Category)CreateEntityFromDictionary(typeof(Category), data);
                    if (categoryEntityRequest == null)
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Incorrect object mapping"));

                    if (categoryEntityRequest.Id.IsNullOrEmpty())
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Category cannot be found"));

                    var category = await _categoryService.GetById(categoryEntityRequest.Id);
                    category.Name = category.Name != categoryEntityRequest.Name ? categoryEntityRequest.Name : category.Name;
                    category.Description = category.Description != categoryEntityRequest.Description ? categoryEntityRequest.Description : category.Description;
                    category.StatusId = category.StatusId != categoryEntityRequest.StatusId ? categoryEntityRequest.StatusId : category.StatusId;
                    category.ModifiedById = currentUser.Id;

                    await _categoryService.Update(category);
                    return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successfully updated Category", categoryEntityRequest));
            }
            #endregion

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(string endpoint, string id)
        {
            // TODO: will add checking of access rights here
            //

            if (string.IsNullOrEmpty(endpoint))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint cannot be empty"));

            var webServiceEndpoint = await _webServiceEndpointService.GetEndpointByEndpointName(endpoint);
            if (webServiceEndpoint == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint doesn't exists, create this endpoint in Web Service Endpoints screen"));

            var currentUser = await GetCurrentUser();
            if (currentUser == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "User has not yet logged in"));

            if (string.IsNullOrEmpty(id))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Id cannot be found"));

            var entityId = Guid.Parse(id);
            if (entityId.IsNullOrEmpty())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Id cannot be empty"));

            #region Switch Domain

            switch (webServiceEndpoint.EndpointDomain)
            {
                case "BusinessEntity":
                    var businessEntity = await _businessEntityService.GetById(entityId);
                    if (businessEntity != null)
                    {
                        await _businessEntityService.Delete(businessEntity);
                        return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successfully Deleted Business Entity", businessEntity));
                    }
                    else
                    {
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Business Entity doesn't exists"));
                    }

                case "Category":
                    var categoryEntity = await _categoryService.GetById(entityId);
                    if (categoryEntity != null)
                    {
                        await _categoryService.Delete(categoryEntity);
                        return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successful Deleted Category", categoryEntity));
                    }
                    else
                    {
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Category doesn't exists"));
                    }
            }
            #endregion

            return Ok();
        }

        #endregion

        #region Methods

        private WebServiceEndpointResponseEntityListModel<TEntity> GenerateEntityListResponseModel<TEntity>(HttpStatusCode statusCode, string message, IEnumerable<TEntity> dataList = null)
            where TEntity : BaseEntity
        {
            var responseModel = new WebServiceEndpointResponseEntityListModel<TEntity>();
            responseModel.Message = message;
            switch (statusCode)
            {
                case HttpStatusCode.OK:
                    responseModel.Status = HttpStatusCode.OK.ToString();
                    break;

                case HttpStatusCode.NotFound:
                    responseModel.Status= HttpStatusCode.NotFound.ToString();
                    break;

                case HttpStatusCode.Unauthorized:
                    responseModel.Status = HttpStatusCode.Unauthorized.ToString();
                    break;
            }

            if (dataList.Any() && dataList != null)
            {
                responseModel.Data = dataList.ToList();
            }

            return responseModel;
        }

        private WebServiceEndpointResponseEntityModel<TEntity> GenerateEntityResponseModel<TEntity>(HttpStatusCode statusCode, string message, TEntity data = null)
            where TEntity : BaseEntity
        {
            var responseModel = new WebServiceEndpointResponseEntityModel<TEntity>();
            responseModel.Message = message;
            switch (statusCode)
            {
                case HttpStatusCode.OK:
                    responseModel.Status = HttpStatusCode.OK.ToString();
                    break;

                case HttpStatusCode.NotFound:
                    responseModel.Status = HttpStatusCode.NotFound.ToString();
                    break;

                case HttpStatusCode.Unauthorized:
                    responseModel.Status = HttpStatusCode.Unauthorized.ToString();
                    break;
            }

            if (data != null)
            {
                responseModel.Data = data;
            }

            return responseModel;
        }

        private WebServiceEndpointResponseErrorModel GenerateErrorResponseModel(HttpStatusCode statusCode, string message)
        {
            var errorResponseModel = new WebServiceEndpointResponseErrorModel();
            errorResponseModel.Message = message;
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

        private async Task<User> GetCurrentUser()
        {
            var user = HttpContext.User;
            User currentUser = null;
            if (user.Identity is { IsAuthenticated: true })
            {
                var userId = user.Identity.Name;
                if (string.IsNullOrEmpty(userId))
                    return currentUser;

                var convertedUserId = Guid.Parse(userId);
                currentUser = await _userService.GetById(convertedUserId);
            }

            return currentUser;
        }

        private object CreateEntityFromDictionary(Type entityType, Dictionary<string, object> data)
        {
            var entity = Activator.CreateInstance(entityType);
            foreach (var kv in data)
            {
                var prop = entityType.GetProperty(kv.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (prop != null && kv.Value != null)
                {
                    var jsonValueKind = ((JsonElement)kv.Value).ValueKind;
                    var valueString = kv.Value.ToString();
                    switch (jsonValueKind)
                    {
                        case JsonValueKind.String:
                            // need to check if Guid or Date or String
                            if (Guid.TryParse(valueString, out Guid guidResult))
                            {
                                if (guidResult.IsNotNullOrEmpty())
                                    prop.SetValue(entity, guidResult);
                            }
                            else if (DateTime.TryParse(valueString, out DateTime dateTimeResult))
                            {
                                prop.SetValue(entity, dateTimeResult);
                            }
                            else
                            {
                                prop.SetValue(entity, valueString);
                            }
                            break;
                        case JsonValueKind.Number:
                            prop.SetValue(entity, JsonConvert.DeserializeObject<int>(kv.Value.ToString()));
                            break;
                        case JsonValueKind.True:
                            prop.SetValue(entity, bool.Parse(valueString));
                            break;
                        case JsonValueKind.False:
                            prop.SetValue(entity, bool.Parse(valueString));
                            break;
                    }   
                }
            }
            return entity;
        }

        #endregion
    }
}
