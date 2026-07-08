using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RA.BusinessEntities.Data;
using RA.BusinessEntities.Domain;
using RA.BusinessEntities.Services;
using RA.Catalogs.Services;
using RA.Categories.Data;
using RA.Categories.Domain;
using RA.Categories.Services;
using RA.Core.Domain;
using RA.Core.Models.PluginModels.BusinessEntities;
using RA.Core.Models.PluginModels.Catalogs;
using RA.Core.Models.PluginModels.Categories;
using RA.Data.Domain.Addresses;
using RA.Data.Domain.Users;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;
using RA.WebServiceEndpoints.Models;
using RA.WebServiceEndpoints.Services;
using RAerp.Helpers.AddressHelper;
using RAerp.Services.AddressServices;
using RAerp.Services.UserServices;
using System;
using System.Collections;
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
    /// <summary>
    /// This is for Entity Types only
    /// </summary>
    [ApiController]
    [Route("api/[controller]/{endpoint}")]
    public class WebServiceEndpointsAPIController : ControllerBase
    {
        #region Constants
        private readonly IWebServiceEndpointService _webServiceEndpointService;
        private readonly IEntityTypeManager _entityTypeManager;
        private readonly IBusinessEntityService _businessEntityService;
        private readonly IUserService _userService;
        private readonly ICategoryService _categoryService;
        private readonly ICatalogService _catalogService;
        private readonly IMapper _mapper;
        private readonly IAddressService _addressService;
        #endregion

        #region Ctor
        public WebServiceEndpointsAPIController(IWebServiceEndpointService webServiceEndpointService,
            IEntityTypeManager entityTypeManager,
            IBusinessEntityService businessEntityService,
            IUserService userService,
            ICategoryService categoryService,
            ICatalogService catalogService,
            IMapper mapper,
            IAddressService addressService)
        {
            _webServiceEndpointService = webServiceEndpointService;
            _entityTypeManager = entityTypeManager;
            _businessEntityService = businessEntityService;
            _userService = userService;
            _categoryService = categoryService;
            _catalogService = catalogService;
            _mapper = mapper;
            _addressService = addressService;
        }
        #endregion

        #region CRUD

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetList(string endpoint)
        {
            // TODO: will add checking of access rights here
            if (string.IsNullOrEmpty(endpoint))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint cannot be empty"));

            var webServiceEndpoint = await _webServiceEndpointService.GetEndpointByEndpointName(endpoint);
            if (webServiceEndpoint == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint doesn't exists, create this endpoint in Web Service Endpoints screen"));
            
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "User has not yet logged in"));

            var requestQuery = Request.Query;
            #region Switch Domain

            switch (webServiceEndpoint.EndpointDomain)
            {
                case "BusinessEntity":
                    BusinessEntitySearchModel businessEntitySearchModel = new BusinessEntitySearchModel();
                    businessEntitySearchModel = (BusinessEntitySearchModel)CreateEntityFromDictionary(typeof(BusinessEntitySearchModel), requestQuery.ToDictionary(x => x.Key, x => (object)x.Value.ToString()));
                    var businessEntityList = await _businessEntityService.GetListAsync(webServiceEndpoint.EndpointEntityTypeId.Value);
                    if (businessEntityList.Any())
                        return Ok(GenerateEntityListResponseModel(HttpStatusCode.OK, "Successful GET", dataList: businessEntityList));
                    break;

                case "Category":
                    CategorySearchModel categorySearchModel = new CategorySearchModel();
                    categorySearchModel = (CategorySearchModel)CreateEntityFromDictionary(typeof(CategorySearchModel), requestQuery.ToDictionary(x => x.Key, x => (object)x.Value.ToString()));
                    var categoryEntityList = await _categoryService.GetListAsync(webServiceEndpoint.EndpointEntityTypeId.Value);
                    if (categoryEntityList.Any())
                        return Ok(GenerateEntityListResponseModel(HttpStatusCode.OK, "Successful GET", dataList: categoryEntityList));
                    break;

                case "Catalog":
                    CatalogSearchModel catalogSearchModel = new CatalogSearchModel();
                    catalogSearchModel = (CatalogSearchModel)CreateEntityFromDictionary(typeof(CatalogSearchModel), requestQuery.ToDictionary(x => x.Key, x => (object)x.Value.ToString()));
                    if (catalogSearchModel.PageNumber == 0 && catalogSearchModel.PageSize == 0)
                    {
                        var catalogEntityList = await _catalogService.GetCatalogListAsync(webServiceEndpoint.EndpointEntityTypeId.Value);
                        if (catalogEntityList.Any())
                            return Ok(GenerateEntityListResponseModel(HttpStatusCode.OK, "Successful GET", dataList: catalogEntityList));
                    }
                    else
                    {
                        var catalogEntityList = await _catalogService.GetCatalogPagedResultListAsync(webServiceEndpoint.EndpointEntityTypeId.Value, pageNumber: catalogSearchModel.PageNumber, pageSize: catalogSearchModel.PageSize);
                        if (catalogEntityList != null)
                            return Ok(GenerateEntityListResponseModel(HttpStatusCode.OK, "Successful GET", catalogSearchModel.PageNumber, catalogSearchModel.PageSize, catalogEntityList.Items));
                    }
                    
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

            var currentUser = await GetCurrentUserAsync();
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
                    var businessEntity = await _businessEntityService.GetByIdAsync(entityId);
                    if (businessEntity != null)
                        return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successful GET", businessEntity));
                    break;

                case "Category":
                    var categoryEntity = await _categoryService.GetByIdAsync(entityId);
                    if (categoryEntity != null)
                        return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successful GET", categoryEntity));
                    break;

                case "Catalog":
                    var catalogEntity = await _catalogService.GetByIdAsync(entityId);
                    if (catalogEntity != null)
                        return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successful GET", catalogEntity));
                    break;
            }
            #endregion

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string endpoint, [FromBody] Dictionary<string, object> data)
        {
            // TODO: will add checking of access rights here
            //

            if (string.IsNullOrEmpty(endpoint))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint cannot be empty"));

            var webServiceEndpoint = await _webServiceEndpointService.GetEndpointByEndpointName(endpoint);
            if (webServiceEndpoint == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint doesn't exists, create this endpoint in Web Service Endpoints screen"));

            var currentUser = await GetCurrentUserAsync();
            //if (currentUser == null)
            //    return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "User has not yet logged in"));

            if (data == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Request body cannot be empty."));

            #region Switch Domain

            switch (webServiceEndpoint.EndpointDomain)
            {
                #region Entity Types

                case "BusinessEntity":
                    var businessEntityRequestModel = (BusinessEntityModel)CreateEntityFromDictionary(typeof(BusinessEntityModel), data);
                    if (businessEntityRequestModel == null)
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Incorrect object mapping"));

                    if (businessEntityRequestModel.CreatedById.IsNullOrEmpty() && currentUser == null)
                        return Unauthorized();

                    var businessEntitySettings = await _entityTypeManager.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(businessEntityRequestModel.EntityTypeId);
                    if (businessEntitySettings != null)
                    {
                        if (businessEntitySettings.AutoGeneratedTemplate)
                            businessEntityRequestModel.Code = null;

                        if (businessEntitySettings.AddressEnabled)
                        {
                            Address address = AddressOverviewModelHelper.PrepareAddressEntity(businessEntityRequestModel.Address);
                            if (address != null && address.Id.IsNullOrEmpty())
                            {
                                if (currentUser != null)
                                    address.CreatedById = currentUser.Id;
                                if (businessEntityRequestModel.CreatedById.IsNotNullOrEmpty())
                                    address.CreatedById = businessEntityRequestModel.CreatedById;
                                address = await _addressService.Insert(address);
                                businessEntityRequestModel.Address.Id = address.Id;
                            }
                        }
                    }

                    if (currentUser != null)
                        businessEntityRequestModel.CreatedById = currentUser.Id;
                    businessEntityRequestModel.ModifiedOn = null;
                    businessEntityRequestModel.ModifiedById = null;
                    if (webServiceEndpoint.EndpointEntityTypeId.IsNotNullOrEmpty())
                        businessEntityRequestModel.EntityTypeId = webServiceEndpoint.EndpointEntityTypeId.Value;

                    var businessEntity = _mapper.Map<BusinessEntity>(businessEntityRequestModel);
                    if (businessEntity == null) return NotFound();

                    if (businessEntityRequestModel.Address != null && businessEntityRequestModel.Address.Id.IsNotNullOrEmpty())
                    {
                        businessEntity.AddressId = businessEntityRequestModel.Address.Id;
                    }

                    await _businessEntityService.InsertAsync(businessEntity);
                    return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successfully created Business Entity", businessEntity));

                case "Category":
                    var categoryEntityRequest = (Category)CreateEntityFromDictionary(typeof(Category), data);
                    if (categoryEntityRequest == null)
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Incorrect object mapping"));

                    var categoryEntitySettings = await _entityTypeManager.GetSettingDataOfEntityAsync<Category, CategorySetting>(categoryEntityRequest.EntityTypeId);
                    if (categoryEntitySettings != null)
                    {
                        if (categoryEntitySettings.AutoGeneratedTemplate)
                            categoryEntityRequest.Code = null;
                    }

                    categoryEntityRequest.CreatedById = currentUser.Id;
                    categoryEntityRequest.ModifiedOn = null;
                    categoryEntityRequest.ModifiedById = null;

                    await _categoryService.InsertAsync(categoryEntityRequest);
                    return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successfully created Category", categoryEntityRequest));

                #endregion
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

            var currentUser = await GetCurrentUserAsync();
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

                    var businessEntity = await _businessEntityService.GetByIdAsync(businessEntityRequest.Id);
                    businessEntity.Name = businessEntity.Name != businessEntityRequest.Name ? businessEntityRequest.Name : businessEntity.Name;
                    businessEntity.Description = businessEntity.Description != businessEntityRequest.Description ? businessEntityRequest.Description : businessEntity.Description;
                    businessEntity.StatusId = businessEntity.StatusId != businessEntityRequest.StatusId ? businessEntityRequest.StatusId : businessEntity.StatusId;
                    businessEntity.CategoryId = businessEntity.CategoryId != businessEntityRequest.CategoryId ? businessEntityRequest.CategoryId : businessEntity.CategoryId;
                    businessEntityRequest.ModifiedById = currentUser.Id;

                    await _businessEntityService.UpdateAsync(businessEntityRequest);
                    return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successfully updated Business Entity", businessEntityRequest));

                case "Category":
                    var categoryEntityRequest = (Category)CreateEntityFromDictionary(typeof(Category), data);
                    if (categoryEntityRequest == null)
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Incorrect object mapping"));

                    if (categoryEntityRequest.Id.IsNullOrEmpty())
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Category cannot be found"));

                    var category = await _categoryService.GetByIdAsync(categoryEntityRequest.Id);
                    category.Name = category.Name != categoryEntityRequest.Name ? categoryEntityRequest.Name : category.Name;
                    category.Description = category.Description != categoryEntityRequest.Description ? categoryEntityRequest.Description : category.Description;
                    category.StatusId = category.StatusId != categoryEntityRequest.StatusId ? categoryEntityRequest.StatusId : category.StatusId;
                    category.ModifiedById = currentUser.Id;

                    await _categoryService.UpdateAsync(category);
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

            var currentUser = await GetCurrentUserAsync();
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
                    var businessEntity = await _businessEntityService.GetByIdAsync(entityId);
                    if (businessEntity != null)
                    {
                        await _businessEntityService.DeleteAsync(businessEntity);
                        return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successfully Deleted Business Entity", businessEntity));
                    }
                    else
                    {
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Business Entity doesn't exists"));
                    }

                case "Category":
                    var categoryEntity = await _categoryService.GetByIdAsync(entityId);
                    if (categoryEntity != null)
                    {
                        await _categoryService.DeleteAsync(categoryEntity);
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

        private WebServiceEndpointResponseEntityListModel<TEntity> GenerateEntityListResponseModel<TEntity>(HttpStatusCode statusCode, string message, int? pageNumber = null, int? pageSize = null, IEnumerable<TEntity> dataList = null)
            where TEntity : BaseEntity
        {
            var responseModel = new WebServiceEndpointResponseEntityListModel<TEntity>();
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

        private WebServiceEndpointResponseEntityModel<TEntity> GenerateEntityResponseModel<TEntity>(HttpStatusCode statusCode, string message, TEntity data = null)
            where TEntity : BaseEntity
        {
            var responseModel = new WebServiceEndpointResponseEntityModel<TEntity>();
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

        private WebServiceEndpointResponseErrorModel GenerateErrorResponseModel(HttpStatusCode statusCode, string message)
        {
            var errorResponseModel = new WebServiceEndpointResponseErrorModel();
            errorResponseModel.Message = message;
            switch (statusCode)
            {
                case HttpStatusCode.OK:
                    errorResponseModel.Status = HttpStatusCode.OK.ToString();
                    errorResponseModel.Success = true;
                    break;

                case HttpStatusCode.NotFound:
                    errorResponseModel.Status = HttpStatusCode.NotFound.ToString();
                    errorResponseModel.Success = false;
                    break;

                case HttpStatusCode.Unauthorized:
                    errorResponseModel.Status = HttpStatusCode.Unauthorized.ToString();
                    errorResponseModel.Success = false;
                    break;
            }
            return errorResponseModel;
        }

        private async Task<User> GetCurrentUserAsync()
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

        //private object CreateEntityFromDictionary(Type entityType, Dictionary<string, object> data)
        //{
        //    var entity = Activator.CreateInstance(entityType);
        //    foreach (var kv in data)
        //    {
        //        var prop = entityType.GetProperty(kv.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        //        if (prop != null && kv.Value != null)
        //        {
        //            var jsonValueKind = ((JsonElement)kv.Value).ValueKind;
        //            var valueString = kv.Value.ToString();
        //            switch (jsonValueKind)
        //            {
        //                case JsonValueKind.String:
        //                    // need to check if Guid or Date or String
        //                    if (Guid.TryParse(valueString, out Guid guidResult))
        //                    {
        //                        if (guidResult.IsNotNullOrEmpty())
        //                            prop.SetValue(entity, guidResult);
        //                    }
        //                    else if (DateTime.TryParse(valueString, out DateTime dateTimeResult))
        //                    {
        //                        prop.SetValue(entity, dateTimeResult);
        //                    }
        //                    else
        //                    {
        //                        prop.SetValue(entity, valueString);
        //                    }
        //                    break;
        //                case JsonValueKind.Number:
        //                    prop.SetValue(entity, JsonConvert.DeserializeObject<int>(kv.Value.ToString()));
        //                    break;
        //                case JsonValueKind.True:
        //                    prop.SetValue(entity, bool.Parse(valueString));
        //                    break;
        //                case JsonValueKind.False:
        //                    prop.SetValue(entity, bool.Parse(valueString));
        //                    break;
        //            }   
        //        }
        //    }
        //    return entity;
        //}

        private object CreateEntityFromDictionary(Type entityType, IDictionary<string, object> data)
        {
            var entity = Activator.CreateInstance(entityType);

            foreach (var kv in data)
            {
                var prop = entityType.GetProperty(
                    kv.Key,
                    BindingFlags.IgnoreCase |
                    BindingFlags.Public |
                    BindingFlags.Instance);

                if (prop == null || kv.Value == null)
                    continue;

                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                object convertedValue = ConvertValue(kv.Value.ToString(), targetType);

                if (convertedValue != null)
                {
                    prop.SetValue(entity, convertedValue);
                }
            }

            return entity;
        }

        private object ConvertValue(string value, Type targetType)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // Handle List<T>
            if (targetType.IsGenericType &&
                targetType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var itemType = targetType.GetGenericArguments()[0];

                var values = value.Split(',', StringSplitOptions.RemoveEmptyEntries);

                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType))!;

                foreach (var v in values)
                {
                    list.Add(ConvertSingle(v.Trim(), itemType));
                }

                return list;
            }
            // Handle Object or Class
            else if (targetType.IsClass && targetType != typeof(string))
            {
                return System.Text.Json.JsonSerializer.Deserialize(value, targetType,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return ConvertSingle(value, targetType);
        }

        private object ConvertSingle(string value, Type targetType)
        {
            if (targetType == typeof(Guid) &&
                Guid.TryParse(value, out var guid))
                return guid;

            if (targetType == typeof(int) &&
                int.TryParse(value, out var i))
                return i;

            if (targetType == typeof(long) &&
                long.TryParse(value, out var l))
                return l;

            if (targetType == typeof(bool) &&
                bool.TryParse(value, out var b))
                return b;

            if (targetType == typeof(DateTime) &&
                DateTime.TryParse(value, out var d))
                return d;

            if (targetType.IsEnum)
                return Enum.Parse(targetType, value, true);

            return value;
        }

        #endregion
    }
}
