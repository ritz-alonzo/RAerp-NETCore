#region Namespaces
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RA.BusinessEntities.Data;
using RA.BusinessEntities.Domain;
using RA.BusinessEntities.DTO;
using RA.BusinessEntities.Services;
using RA.BusinessEntities.Validators;
using RA.Catalogs.Data;
using RA.Catalogs.Domain;
using RA.Catalogs.DTO;
using RA.Catalogs.Services;
using RA.Categories.Data;
using RA.Categories.Domain;
using RA.Categories.DTO;
using RA.Categories.Services;
using RA.Core.Domain;
using RA.Core.Models.PluginModels.BusinessEntities;
using RA.Core.Models.PluginModels.Catalogs;
using RA.Core.Models.PluginModels.Categories;
using RA.Core.PluginData.EntityTypes.Catalogs;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;
using RA.WebServiceEndpoints.Models;
using RA.WebServiceEndpoints.Services;
using RAerp.Controllers.Admin;
using RAerp.Domain.Addresses;
using RAerp.Domain.Application;
using RAerp.Domain.EntityAttributes;
using RAerp.Domain.Users;
using RAerp.Helpers.AddressHelper;
using RAerp.Helpers.Security;
using RAerp.Helpers.UserHelper;
using RAerp.Models.ApiModel;
using RAerp.Security.AccessRightsControl;
using RAerp.Services.AddressServices;
using RAerp.Services.ApplicationSettingServices;
using RAerp.Services.EntityAttributeServices;
using RAerp.Services.FileServices;
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
#endregion
// 04-21-25 to have:
// 1. Need to add JWT Token generation and validation - done (04-23-25)
// 2. Need to add Access Rights
namespace RA.WebServiceEndpoints.Controllers
{
    /// <summary>
    /// This is for Entity Types only
    /// </summary>
    [ApiController]
    [Route("api/webservice/{endpoint}")]
    public class WebServiceEndpointsAPIController : AdminApiController
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
        private readonly IApplicationSettingService _applicationSettingService;
        private readonly ApplicationSetting _applicationSetting;
        private readonly IAccessControl _accessControl;
        private readonly IUserIdentity _userIdentity;
        private readonly IFileService _fileService;
        private readonly IEntityAttributeService _entityAttributeService;
        #endregion

        #region Ctor
        public WebServiceEndpointsAPIController(IWebServiceEndpointService webServiceEndpointService,
            IEntityTypeManager entityTypeManager,
            IBusinessEntityService businessEntityService,
            IUserService userService,
            ICategoryService categoryService,
            ICatalogService catalogService,
            IMapper mapper,
            IAddressService addressService,
            IApplicationSettingService applicationSettingService,
            IAccessControl accessControl,
            IUserIdentity userIdentity,
            IFileService fileService,
            IEntityAttributeService entityAttributeService)
        {
            _webServiceEndpointService = webServiceEndpointService;
            _entityTypeManager = entityTypeManager;
            _businessEntityService = businessEntityService;
            _userService = userService;
            _categoryService = categoryService;
            _catalogService = catalogService;
            _mapper = mapper;
            _addressService = addressService;
            _applicationSettingService = applicationSettingService;
            _applicationSetting = _applicationSettingService.GetCurrentApplicationSettingAsync()?.Result;
            _accessControl = accessControl;
            _userIdentity = userIdentity;
            _fileService = fileService;
            _entityAttributeService = entityAttributeService;
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

            string clientId = HttpContext.Request.Cookies["client_id"];
            string clientSecret = HttpContext.Request.Cookies["client_secret"];

            if (string.IsNullOrEmpty(clientId))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "No client id in Headers."));

            if (string.IsNullOrEmpty(clientSecret))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "No client secret in Headers."));

            if (clientId != _applicationSetting.ClientId || clientSecret != _applicationSetting.ClientSecret)
                return StatusCode((int)HttpStatusCode.Forbidden, GenerateErrorResponseModel(HttpStatusCode.Forbidden, "Invalid client id or client secret."));

            var webServiceEndpoint = await _webServiceEndpointService.GetEndpointByEndpointName(endpoint);
            if (webServiceEndpoint == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint doesn't exists, create this endpoint in Web Service Endpoints screen"));
            
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                return StatusCode((int)HttpStatusCode.Forbidden, GenerateErrorResponseModel(HttpStatusCode.Forbidden, "User has not yet logged in"));

            var requestQuery = Request.Query;
            #region Switch Domain

            switch (webServiceEndpoint.EndpointDomain)
            {
                case "BusinessEntity":
                    ApiValidationModel businessEntityValidationModel = await ValidateUserAccessAndCredentials<BusinessEntity>(_accessControl, _userIdentity, _applicationSetting);
                    if (businessEntityValidationModel != null && businessEntityValidationModel.IsPassed == false)
                        return StatusCode((int)businessEntityValidationModel.StatusCode, GenerateErrorResponseModel(businessEntityValidationModel.StatusCode, businessEntityValidationModel.Message));

                    BusinessEntityQueryRequestDto businessEntityQueryRequest = new BusinessEntityQueryRequestDto();
                    businessEntityQueryRequest = (BusinessEntityQueryRequestDto)CreateEntityFromDictionary(typeof(BusinessEntityQueryRequestDto), requestQuery.ToDictionary(x => x.Key, x => (object)x.Value.ToString()));

                    BusinessEntitySetting businessEntitySetting = await _entityTypeManager.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(webServiceEndpoint.EndpointEntityTypeId.Value);
                    if (businessEntitySetting == null)
                        return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Business Entity Settings not yet configured."));

                    var businessEntityList = await _businessEntityService.GetListAsync(webServiceEndpoint.EndpointEntityTypeId.Value);
                    
                    List<BusinessEntityResponseDto> businessEntityResponseList = new List<BusinessEntityResponseDto>();
                    if (businessEntityList.Any())
                    {
                        businessEntityResponseList = businessEntityList.Select(businessEntity =>
                        {
                            BusinessEntityResponseDto businessEntityResponse = new BusinessEntityResponseDto();
                            businessEntityResponse = _mapper.Map<BusinessEntityResponseDto>(businessEntity);
                            User createdByUser = _userIdentity.GetUserDetailsAsync(businessEntity.CreatedById).Result;
                            businessEntityResponse.CreatedBy = createdByUser?.FirstName + ' ' + createdByUser?.LastName;
                            User modifiedByUser = businessEntity.ModifiedById.IsNotNullOrEmpty() ? _userIdentity.GetUserDetailsAsync(businessEntity.ModifiedById.Value).Result : null;
                            businessEntityResponse.ModifiedBy = modifiedByUser != null ? modifiedByUser?.FirstName + ' ' + modifiedByUser?.LastName : null;
                            // Attributes
                            businessEntityResponse.Attributes = _entityAttributeService.GetEntityAttributeValueListAsync(entityId: businessEntity.Id).Result.ToList();

                            if (businessEntity.AddressId.IsNotNullOrEmpty())
                            {
                                var address = _addressService.GetById(businessEntity.AddressId.Value).Result;
                                if (address != null)
                                    businessEntityResponse.Address = AddressOverviewModelHelper.PrepareOverviewModel(address);
                            }

                            return businessEntityResponse;

                        }).ToList();

                    }
                    return Ok(GenerateEntityListResponseModel(HttpStatusCode.OK, "Successful GET", dataList: businessEntityResponseList));

                case "Category":
                    ApiValidationModel categoryValidationModel = await ValidateUserAccessAndCredentials<Category>(_accessControl, _userIdentity, _applicationSetting);
                    if (categoryValidationModel != null && categoryValidationModel.IsPassed == false)
                        return StatusCode((int)categoryValidationModel.StatusCode, GenerateErrorResponseModel(categoryValidationModel.StatusCode, categoryValidationModel.Message));

                    CategoryQueryRequestDto categoryQueryRequest = new CategoryQueryRequestDto();
                    categoryQueryRequest = (CategoryQueryRequestDto)CreateEntityFromDictionary(typeof(CategoryQueryRequestDto), requestQuery.ToDictionary(x => x.Key, x => (object)x.Value.ToString()));
                    
                    var categoryEntityList = await _categoryService.GetListAsync(webServiceEndpoint.EndpointEntityTypeId.Value);
                    
                    List<CategoryResponseDto> categoryResponseList = new List<CategoryResponseDto>();
                    if (categoryEntityList.Any())
                    {
                        categoryResponseList = categoryEntityList.Select(category =>
                        {
                            CategoryResponseDto categoryResponse = new CategoryResponseDto();
                            categoryResponse = _mapper.Map<CategoryResponseDto>(category);
                            User createdByUser = _userIdentity.GetUserDetailsAsync(category.CreatedById).Result;
                            categoryResponse.CreatedBy = createdByUser?.FirstName + ' ' + createdByUser?.LastName;
                            User modifiedByUser = category.ModifiedById.IsNotNullOrEmpty() ? _userIdentity.GetUserDetailsAsync(category.ModifiedById.Value).Result : null;
                            categoryResponse.ModifiedBy = modifiedByUser != null ? modifiedByUser?.FirstName + ' ' + modifiedByUser?.LastName : null;
                            // Attributes
                            categoryResponse.Attributes = _entityAttributeService.GetEntityAttributeValueListAsync(entityId: category.Id).Result.ToList();

                            return categoryResponse;

                        }).ToList();
                    }

                    return Ok(GenerateEntityListResponseModel(HttpStatusCode.OK, "Successful GET", dataList: categoryResponseList));

                case "Catalog":
                    ApiValidationModel catalogValidationModel = await ValidateUserAccessAndCredentials<Catalog>(_accessControl, _userIdentity, _applicationSetting);
                    if (catalogValidationModel != null && catalogValidationModel.IsPassed == false)
                        return StatusCode((int)catalogValidationModel.StatusCode, GenerateErrorResponseModel(catalogValidationModel.StatusCode, catalogValidationModel.Message));

                    CatalogQueryRequestDto catalogQueryRequest = new CatalogQueryRequestDto();
                    catalogQueryRequest = (CatalogQueryRequestDto)CreateEntityFromDictionary(typeof(CatalogQueryRequestDto), requestQuery.ToDictionary(x => x.Key, x => (object)x.Value.ToString()));
                    
                    var catalogEntityList = await _catalogService.GetCatalogPagedResultListAsync(webServiceEndpoint.EndpointEntityTypeId.Value,
                                                categoryTypeIds: catalogQueryRequest.SearchCategoryTypeIds,
                                                searchQuery: catalogQueryRequest.SearchQuery,
                                                catalogStatusIds: catalogQueryRequest.SearchStatusIds,
                                                showDeleted: catalogQueryRequest.ShowDeleted,
                                                pageNumber: catalogQueryRequest.PageNumber, pageSize: catalogQueryRequest.PageSize);
                    List<CatalogResponseDto> catalogResponseList = new List<CatalogResponseDto>();
                    if (catalogEntityList.Items.Any())
                    {
                        catalogResponseList = catalogEntityList.Items.Select(catalog =>
                        {
                            CatalogResponseDto catalogResponse = new CatalogResponseDto();
                            catalogResponse = _mapper.Map<CatalogResponseDto>(catalog);
                            User createdByUser = _userIdentity.GetUserDetailsAsync(catalog.CreatedById).Result;
                            catalogResponse.CreatedBy = createdByUser?.FirstName + ' ' + createdByUser?.LastName;
                            User modifiedByUser = catalog.ModifiedById.IsNotNullOrEmpty() ? _userIdentity.GetUserDetailsAsync(catalog.ModifiedById.Value).Result : null;
                            catalogResponse.ModifiedBy = modifiedByUser != null ? modifiedByUser?.FirstName + ' ' + modifiedByUser?.LastName : null;
                            // Attributes
                            catalogResponse.Attributes = _entityAttributeService.GetEntityAttributeValueListAsync(entityId: catalog.Id).Result.ToList();

                            return catalogResponse;

                        }).ToList();

                    }

                    return Ok(GenerateEntityListResponseModel(HttpStatusCode.OK, "Successful GET", dataList: catalogResponseList));
            }
            #endregion

            return Ok();
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetById(string endpoint, Guid id)
        {
            // TODO: will add checking of access rights here
            //
            if (string.IsNullOrEmpty(endpoint))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint cannot be empty"));

            string clientId = HttpContext.Request.Cookies["client_id"];
            string clientSecret = HttpContext.Request.Cookies["client_secret"];

            if (string.IsNullOrEmpty(clientId))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "No client id in Headers."));

            if (string.IsNullOrEmpty(clientSecret))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "No client secret in Headers."));

            if (clientId != _applicationSetting.ClientId || clientSecret != _applicationSetting.ClientSecret)
                return StatusCode((int)HttpStatusCode.Forbidden, GenerateErrorResponseModel(HttpStatusCode.Forbidden, "Invalid client id or client secret."));

            var webServiceEndpoint = await _webServiceEndpointService.GetEndpointByEndpointName(endpoint);
            if (webServiceEndpoint == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint doesn't exists, create this endpoint in Web Service Endpoints screen"));

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                return StatusCode((int)HttpStatusCode.Forbidden, GenerateErrorResponseModel(HttpStatusCode.Forbidden, "User has not yet logged in"));

            if (id.IsNullOrEmpty())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Id cannot be empty"));

            #region Switch Domain

            switch (webServiceEndpoint.EndpointDomain)
            {
                case "BusinessEntity":
                    ApiValidationModel businessEntityValidationModel = await ValidateUserAccessAndCredentials<BusinessEntity>(_accessControl, _userIdentity, _applicationSetting);
                    if (businessEntityValidationModel != null && businessEntityValidationModel.IsPassed == false)
                        return StatusCode((int)businessEntityValidationModel.StatusCode, GenerateErrorResponseModel(businessEntityValidationModel.StatusCode, businessEntityValidationModel.Message));

                    var businessEntity = await _businessEntityService.GetByIdAsync(id);
                    if (businessEntity == null)
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Business Entity cannot be found"));

                    BusinessEntityResponseDto businessEntityResponse = _mapper.Map<BusinessEntityResponseDto>(businessEntity);

                    return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successful GET", businessEntityResponse));

                case "Category":
                    ApiValidationModel categoryValidationModel = await ValidateUserAccessAndCredentials<Category>(_accessControl, _userIdentity, _applicationSetting);
                    if (categoryValidationModel != null && categoryValidationModel.IsPassed == false)
                        return StatusCode((int)categoryValidationModel.StatusCode, GenerateErrorResponseModel(categoryValidationModel.StatusCode, categoryValidationModel.Message));

                    var category = await _categoryService.GetByIdAsync(id);
                    if (category == null)
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Category cannot be found"));

                    CategoryResponseDto categoryResponse = _mapper.Map<CategoryResponseDto>(category);
                    return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successful GET", categoryResponse));

                case "Catalog":
                    ApiValidationModel catalogValidationModel = await ValidateUserAccessAndCredentials<Catalog>(_accessControl, _userIdentity, _applicationSetting);
                    if (catalogValidationModel != null && catalogValidationModel.IsPassed == false)
                        return StatusCode((int)catalogValidationModel.StatusCode, GenerateErrorResponseModel(catalogValidationModel.StatusCode, catalogValidationModel.Message));

                    var catalog = await _catalogService.GetByIdAsync(id);
                    if (catalog == null)
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Catalog cannot be found"));

                    CatalogResponseDto catalogResponse = _mapper.Map<CatalogResponseDto>(catalog);
                    return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successful GET", catalogResponse));
            }
            #endregion

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string endpoint, [FromBody] Dictionary<string, object> data)
        {
            if (string.IsNullOrEmpty(endpoint))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint cannot be empty"));

            string clientId = HttpContext.Request.Cookies["client_id"];
            string clientSecret = HttpContext.Request.Cookies["client_secret"];

            if (string.IsNullOrEmpty(clientId))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "No client id in Headers."));

            if (string.IsNullOrEmpty(clientSecret))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "No client secret in Headers."));

            if (clientId != _applicationSetting.ClientId || clientSecret != _applicationSetting.ClientSecret)
                return StatusCode((int)HttpStatusCode.Forbidden, GenerateErrorResponseModel(HttpStatusCode.Forbidden, "Invalid client id or client secret."));

            var webServiceEndpoint = await _webServiceEndpointService.GetEndpointByEndpointName(endpoint);
            if (webServiceEndpoint == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint doesn't exists, create this endpoint in Web Service Endpoints screen"));

            var currentUser = await GetCurrentUserAsync();

            if (data == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Request body cannot be empty."));

            #region Switch Domain

            switch (webServiceEndpoint.EndpointDomain)
            {
                #region Entity Types

                case "BusinessEntity":
                    var businessEntityRequest = (BusinessEntityRequestDto)CreateEntityFromDictionary(typeof(BusinessEntityRequestDto), data);
                    if (businessEntityRequest == null)
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Incorrect object mapping"));

                    ApiRequestValidationModel requestValidationModel = await ValidateRequestEntityValues<BusinessEntityRequestDto, BusinessEntityDtoValidator>(businessEntityRequest);
                    if (!requestValidationModel.IsValid)
                        return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, requestValidationModel.ValidationMessage));

                    if (businessEntityRequest.CreatedById.IsNullOrEmpty() && currentUser == null)
                        return StatusCode((int)HttpStatusCode.Forbidden, GenerateErrorResponseModel(HttpStatusCode.Forbidden, "User has not yet logged in"));

                    var businessEntitySettings = await _entityTypeManager.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(webServiceEndpoint.EndpointEntityTypeId.Value);
                    if (businessEntitySettings != null)
                    {
                        if (businessEntitySettings.AutoGeneratedTemplate)
                            businessEntityRequest.Code = null;

                        if (businessEntitySettings.AddressEnabled)
                        {
                            Address address = AddressOverviewModelHelper.PrepareAddressEntity(businessEntityRequest.Address);
                            if (address != null && address.Id.IsNullOrEmpty())
                            {
                                if (currentUser != null)
                                    address.CreatedById = currentUser.Id;
                                if (businessEntityRequest.CreatedById.IsNotNullOrEmpty())
                                    address.CreatedById = businessEntityRequest.CreatedById.Value;
                                address = await _addressService.Insert(address);
                                businessEntityRequest.Address.Id = address.Id;
                            }
                        }
                    }

                    if (currentUser != null)
                        businessEntityRequest.CreatedById = currentUser.Id;
                    if (webServiceEndpoint.EndpointEntityTypeId.IsNotNullOrEmpty())
                        businessEntityRequest.EntityTypeId = webServiceEndpoint.EndpointEntityTypeId.Value;

                    BusinessEntity businessEntity = _mapper.Map<BusinessEntity>(businessEntityRequest);
                    if (businessEntity == null) return NotFound();

                    if (businessEntityRequest.Address != null && businessEntityRequest.Address.Id.IsNotNullOrEmpty())
                        businessEntity.AddressId = businessEntityRequest.Address.Id;

                    await _businessEntityService.InsertAsync(businessEntity);

                    // Insert Or Update Attributes if any
                    if (businessEntity.Attributes.Any())
                        await _entityAttributeService.InsertOrUpdateEntityAttributeValuesMappingAsync(businessEntity.Attributes, businessEntity.Id);

                    BusinessEntityResponseDto businessEntityResponse = _mapper.Map<BusinessEntityResponseDto>(businessEntity);
                    if (businessEntity.AddressId.IsNotNullOrEmpty())
                    {
                        var address = await _addressService.GetById(businessEntity.AddressId.Value);
                        businessEntityResponse.Address = AddressOverviewModelHelper.PrepareOverviewModel(address);
                    }

                    return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successfully created Business Entity", businessEntityResponse));

                case "Category":
                    var categoryEntityRequest = (CategoryRequestDto)CreateEntityFromDictionary(typeof(CategoryRequestDto), data);
                    if (categoryEntityRequest == null)
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Incorrect object mapping"));

                    var categoryEntitySettings = await _entityTypeManager.GetSettingDataOfEntityAsync<Category, CategorySetting>(webServiceEndpoint.EndpointEntityTypeId.Value);
                    if (categoryEntitySettings != null)
                    {
                        if (categoryEntitySettings.AutoGeneratedTemplate)
                            categoryEntityRequest.Code = null;
                    }
                    if (webServiceEndpoint.EndpointEntityTypeId.IsNotNullOrEmpty())
                        categoryEntityRequest.EntityTypeId = webServiceEndpoint.EndpointEntityTypeId.Value;

                    Category category = _mapper.Map<Category>(categoryEntityRequest);
                    category.CreatedById = currentUser.Id;

                    await _categoryService.InsertAsync(category);

                    // Insert Or Update Attributes if any
                    if (category.Attributes.Any())
                        await _entityAttributeService.InsertOrUpdateEntityAttributeValuesMappingAsync(category.Attributes, category.Id);

                    CategoryResponseDto categoryResponse = _mapper.Map<CategoryResponseDto>(category);

                    return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successfully created Category", categoryResponse));

                case "Catalog":
                    var catalogEntityRequest = (CatalogRequestDto)CreateEntityFromDictionary(typeof(CatalogRequestDto), data);
                    if (catalogEntityRequest == null)
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Incorrect object mapping"));

                    var catalogEntitySettings = await _entityTypeManager.GetSettingDataOfEntityAsync<Catalog, CatalogSetting>(webServiceEndpoint.EndpointEntityTypeId.Value);
                    if (catalogEntitySettings == null)
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Catalog Settings not yet configured"));

                    if (webServiceEndpoint.EndpointEntityTypeId.IsNotNullOrEmpty())
                        catalogEntityRequest.EntityTypeId = webServiceEndpoint.EndpointEntityTypeId.Value;

                    if (catalogEntitySettings != null && catalogEntitySettings.AutoGeneratedTemplate && !string.IsNullOrEmpty(catalogEntitySettings.SKUTemplate))
                        catalogEntityRequest.Code = null;

                    if (catalogEntitySettings.IsSKUEnabled && !string.IsNullOrEmpty(catalogEntitySettings.SKUTemplate))
                        catalogEntityRequest.SKU = (catalogEntitySettings.TemplateCount + catalogEntitySettings.TemplateIncrementCount).ToString(catalogEntitySettings.SKUTemplate);

                    if (catalogEntitySettings.IsBarcodeEnabled && !string.IsNullOrEmpty(catalogEntitySettings.BarcodeTemplate))
                        catalogEntityRequest.BarcodeValue = (catalogEntitySettings.TemplateCount + catalogEntitySettings.TemplateIncrementCount).ToString(catalogEntityRequest.BarcodeValue);

                    if (catalogEntityRequest.Price < 0)
                        catalogEntityRequest.Price = 0;

                    Catalog catalog = _mapper.Map<Catalog>(catalogEntityRequest);
                    catalog.CreatedById = currentUser.Id;
                    if (catalogEntityRequest.TypeId == 1 || catalogEntityRequest.TypeId == 0)
                        catalog.Type = CatalogType.Product;
                    else
                        catalog.Type = CatalogType.Service;

                    if (catalogEntityRequest.StatusId == 1 || catalogEntityRequest.StatusId == 0)
                        catalog.Status = CatalogStatus.Active;
                    else
                        catalog.Status = CatalogStatus.Inactive;

                    await _catalogService.InsertAsync(catalog);

                    // Insert Or Update Attributes if any
                    if (catalog.Attributes.Any())
                        await _entityAttributeService.InsertOrUpdateEntityAttributeValuesMappingAsync(catalog.Attributes, catalog.Id);

                    CatalogResponseDto catalogResponse = _mapper.Map<CatalogResponseDto>(catalog);
                    catalogResponse.CreatedBy = currentUser?.FirstName + ' ' + currentUser?.LastName;

                    return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successfully created Catalog", catalogResponse));

                #endregion
            }

            #endregion

            return Ok();
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Update(string endpoint, [FromBody] Dictionary<string, object> data)
        {
            if (string.IsNullOrEmpty(endpoint))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint cannot be empty"));

            string clientId = HttpContext.Request.Cookies["client_id"];
            string clientSecret = HttpContext.Request.Cookies["client_secret"];

            if (string.IsNullOrEmpty(clientId))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "No client id in Headers."));

            if (string.IsNullOrEmpty(clientSecret))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "No client secret in Headers."));

            if (clientId != _applicationSetting.ClientId || clientSecret != _applicationSetting.ClientSecret)
                return StatusCode((int)HttpStatusCode.Forbidden, GenerateErrorResponseModel(HttpStatusCode.Forbidden, "Invalid client id or client secret."));

            var webServiceEndpoint = await _webServiceEndpointService.GetEndpointByEndpointName(endpoint);
            if (webServiceEndpoint == null)
                return StatusCode((int)HttpStatusCode.Forbidden, GenerateErrorResponseModel(HttpStatusCode.Forbidden, "Endpoint doesn't exists, create this endpoint in Web Service Endpoints screen"));

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                return StatusCode((int)HttpStatusCode.Forbidden, GenerateErrorResponseModel(HttpStatusCode.Forbidden, "User has not yet logged in"));

            if (data == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Request body cannot be empty."));

            #region Switch Domain

            switch (webServiceEndpoint.EndpointDomain)
            {
                case "BusinessEntity":
                    var businessEntityRequest = (BusinessEntityRequestDto)CreateEntityFromDictionary(typeof(BusinessEntityRequestDto), data);
                    if (businessEntityRequest == null)
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Incorrect object mapping"));

                    if (businessEntityRequest.Id.IsNullOrEmpty())
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Business Entity cannot be found"));

                    if (webServiceEndpoint.EndpointEntityTypeId.IsNotNullOrEmpty())
                        businessEntityRequest.EntityTypeId = webServiceEndpoint.EndpointEntityTypeId.Value;

                    var existingBusinessEntity = await _businessEntityService.GetByIdAsync(businessEntityRequest.Id);
                    if (existingBusinessEntity == null)
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Business entity cannot be found"));

                    BusinessEntity businessEntity = _mapper.Map(businessEntityRequest, existingBusinessEntity);
                    businessEntity.ModifiedById = currentUser.Id;

                    await _businessEntityService.UpdateAsync(businessEntity);

                    // Insert Or Update Attributes if any
                    if (businessEntity.Attributes.Any())
                        await _entityAttributeService.InsertOrUpdateEntityAttributeValuesMappingAsync(businessEntity.Attributes, businessEntity.Id);

                    BusinessEntityResponseDto businessEntityResponse = _mapper.Map<BusinessEntityResponseDto>(businessEntity);
                    User businessEntityCreatedBy = await _userIdentity.GetUserDetailsAsync(businessEntity.CreatedById);
                    businessEntityResponse.CreatedBy = businessEntityCreatedBy?.FirstName + ' ' + businessEntityCreatedBy?.LastName;
                    businessEntityResponse.ModifiedBy = currentUser?.FirstName + ' ' + currentUser?.LastName;

                    return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successfully updated Business Entity", businessEntityRequest));

                case "Category":
                    var categoryEntityRequest = (CategoryRequestDto)CreateEntityFromDictionary(typeof(CategoryRequestDto), data);
                    if (categoryEntityRequest == null)
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Incorrect object mapping"));
                    if (categoryEntityRequest.Id.IsNullOrEmpty())
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Category cannot be found"));

                    if (webServiceEndpoint.EndpointEntityTypeId.IsNotNullOrEmpty())
                        categoryEntityRequest.EntityTypeId = webServiceEndpoint.EndpointEntityTypeId.Value;

                    var existingCategory = await _categoryService.GetByIdAsync(categoryEntityRequest.Id);
                    if (existingCategory == null)
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Category doesn't exists"));

                    Category category = _mapper.Map(categoryEntityRequest, existingCategory);
                    category.ModifiedById = currentUser.Id;

                    await _categoryService.UpdateAsync(existingCategory);

                    // Insert Or Update Attributes if any
                    if (category.Attributes.Any())
                        await _entityAttributeService.InsertOrUpdateEntityAttributeValuesMappingAsync(category.Attributes, category.Id);

                    CategoryResponseDto categoryResponse = _mapper.Map<CategoryResponseDto>(category);
                    User categoryCreatedBy = await _userIdentity.GetUserDetailsAsync(category.CreatedById);
                    categoryResponse.CreatedBy = categoryCreatedBy?.FirstName + ' ' + categoryCreatedBy?.LastName;
                    categoryResponse.ModifiedBy = currentUser?.FirstName + ' ' + currentUser?.LastName;

                    return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successfully updated Category", categoryEntityRequest));

                case "Catalog":
                    var catalogEntityRequest = (CatalogRequestDto)CreateEntityFromDictionary(typeof(CatalogRequestDto), data);
                    if (catalogEntityRequest == null)
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Incorrect object mapping"));
                    if (catalogEntityRequest.Id.IsNullOrEmpty())
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Catalog doesn't exists"));

                    CatalogSetting catalogEntitySettings = await _entityTypeManager.GetSettingDataOfEntityAsync<Catalog, CatalogSetting>(webServiceEndpoint.EndpointEntityTypeId.Value);

                    if (webServiceEndpoint.EndpointEntityTypeId.IsNotNullOrEmpty())
                        catalogEntityRequest.EntityTypeId = webServiceEndpoint.EndpointEntityTypeId.Value;

                    Catalog existingCatalog = await _catalogService.GetByIdAsync(catalogEntityRequest.Id);
                    if (existingCatalog == null)
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Catalog doesn't exists"));

                    Catalog catalog = _mapper.Map(catalogEntityRequest, existingCatalog);
                    
                    catalog.ModifiedById = currentUser.Id;

                    await _catalogService.UpdateAsync(catalog);

                    // Insert Or Update Attributes if any
                    if (catalog.Attributes.Any())
                        await _entityAttributeService.InsertOrUpdateEntityAttributeValuesMappingAsync(catalog.Attributes, catalog.Id);

                    CatalogResponseDto catalogResponse = _mapper.Map<CatalogResponseDto>(catalog);
                    User catalogCreatedBy = await _userIdentity.GetUserDetailsAsync(catalog.CreatedById);
                    catalogResponse.CreatedBy = catalogCreatedBy?.FirstName + ' ' + catalogCreatedBy?.LastName;
                    catalogResponse.ModifiedBy = currentUser?.FirstName + ' ' + currentUser?.LastName;

                    return Ok(GenerateEntityResponseModel(HttpStatusCode.OK, "Successfully updated Catalog", catalogResponse));
            }
            #endregion

            return Ok();
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> Delete(string endpoint, Guid id)
        {
            if (string.IsNullOrEmpty(endpoint))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint cannot be empty"));

            string clientId = HttpContext.Request.Cookies["client_id"];
            string clientSecret = HttpContext.Request.Cookies["client_secret"];

            if (string.IsNullOrEmpty(clientId))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "No client id in Headers."));

            if (string.IsNullOrEmpty(clientSecret))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "No client secret in Headers."));

            if (clientId != _applicationSetting.ClientId || clientSecret != _applicationSetting.ClientSecret)
                return StatusCode((int)HttpStatusCode.Forbidden, GenerateErrorResponseModel(HttpStatusCode.Forbidden, "Invalid client id or client secret."));

            var webServiceEndpoint = await _webServiceEndpointService.GetEndpointByEndpointName(endpoint);
            if (webServiceEndpoint == null)
                return StatusCode((int)HttpStatusCode.Forbidden, GenerateErrorResponseModel(HttpStatusCode.Forbidden, "Endpoint doesn't exists, create this endpoint in Web Service Endpoints screen"));

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                return StatusCode((int)HttpStatusCode.Forbidden, GenerateErrorResponseModel(HttpStatusCode.Forbidden, "User has not yet logged in"));

            if (id.IsNullOrEmpty())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Id cannot be empty"));

            #region Switch Domain

            switch (webServiceEndpoint.EndpointDomain)
            {
                case "BusinessEntity":
                    var businessEntity = await _businessEntityService.GetByIdAsync(id);
                    if (businessEntity != null)
                    {
                        await _businessEntityService.DeleteAsync(businessEntity);
                        return Ok("Successfully Deleted Business Entity");
                    }
                    else
                    {
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Business Entity doesn't exists"));
                    }

                case "Category":
                    var categoryEntity = await _categoryService.GetByIdAsync(id);
                    if (categoryEntity != null)
                    {
                        await _categoryService.DeleteAsync(categoryEntity);
                        return Ok("Successful Deleted Category");
                    }
                    else
                    {
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Category doesn't exists"));
                    }

                case "Catalog":
                    var catalogEntity = await _catalogService.GetByIdAsync(id);
                    if (catalogEntity != null)
                    {
                        await _catalogService.DeleteAsync(catalogEntity);
                        return Ok("Successfully deleted Catalog");
                    }
                    else
                    {
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Catalog doesn't exists"));
                    }
            }
            #endregion

            return Ok();
        }

        #region Customer Endpoint
        [HttpGet("customer/current")]
        [Authorize]
        public async Task<IActionResult> GetCurrentCustomer(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint cannot be empty"));

            string clientId = HttpContext.Request.Cookies["client_id"];
            string clientSecret = HttpContext.Request.Cookies["client_secret"];

            if (string.IsNullOrEmpty(clientId))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "No client id in Headers."));

            if (string.IsNullOrEmpty(clientSecret))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "No client secret in Headers."));

            if (clientId != _applicationSetting.ClientId || clientSecret != _applicationSetting.ClientSecret)
                return StatusCode((int)HttpStatusCode.Forbidden, GenerateErrorResponseModel(HttpStatusCode.Forbidden, "Invalid client id or client secret."));

            var webServiceEndpoint = await _webServiceEndpointService.GetEndpointByEndpointName(endpoint);
            if (webServiceEndpoint == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint doesn't exists, create this endpoint in Web Service Endpoints screen"));

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                return StatusCode((int)HttpStatusCode.Forbidden, GenerateErrorResponseModel(HttpStatusCode.Forbidden, "User has not yet logged in"));

            if (webServiceEndpoint.EndpointDomain != nameof(BusinessEntity))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint domain is not BusinessEntity"));

            BusinessEntity customerEntity = await _businessEntityService.GetBusinessEntityByUserId(currentUser.Id);
            if (customerEntity == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Customer entity not found for the current user"));

            BusinessEntityCustomerResponseDto customerResponse = new BusinessEntityCustomerResponseDto();
            customerResponse.Id = customerEntity.Id;
            customerResponse.Name = customerEntity.Name;
            customerResponse.Description = customerEntity.Description;
            if (!string.IsNullOrEmpty(currentUser.Email))
                customerResponse.Email = await EncryptionHelper.DecryptData(currentUser.Email, currentUser.Salt);
            if (!string.IsNullOrEmpty(currentUser.ContactNo))
                customerResponse.ContactNo = await EncryptionHelper.DecryptData(currentUser.ContactNo, currentUser.Salt);
            customerResponse.Address = customerEntity.AddressId.IsNotNullOrEmpty() ? AddressOverviewModelHelper.PrepareOverviewModel(await _addressService.GetById(customerEntity.AddressId.Value)) : null;

            return Ok(GenerateResponseModel(HttpStatusCode.OK, "Successfully get customer", customerResponse));
        }

        [HttpPut("customer/current")]
        [Authorize]
        public async Task<IActionResult> UpdateCurrentCustomer(string endpoint, [FromBody] BusinessEntityCustomerRequestDto customerRequestDto)
        {
            if (string.IsNullOrEmpty(endpoint))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint cannot be empty"));

            string clientId = HttpContext.Request.Cookies["client_id"];
            string clientSecret = HttpContext.Request.Cookies["client_secret"];

            if (string.IsNullOrEmpty(clientId))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "No client id in Headers."));

            if (string.IsNullOrEmpty(clientSecret))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "No client secret in Headers."));

            if (clientId != _applicationSetting.ClientId || clientSecret != _applicationSetting.ClientSecret)
                return StatusCode((int)HttpStatusCode.Forbidden, GenerateErrorResponseModel(HttpStatusCode.Forbidden, "Invalid client id or client secret."));

            var webServiceEndpoint = await _webServiceEndpointService.GetEndpointByEndpointName(endpoint);
            if (webServiceEndpoint == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint doesn't exists, create this endpoint in Web Service Endpoints screen"));

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                return StatusCode((int)HttpStatusCode.Forbidden, GenerateErrorResponseModel(HttpStatusCode.Forbidden, "User has not yet logged in"));

            if (webServiceEndpoint.EndpointDomain != nameof(BusinessEntity))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Endpoint domain is not BusinessEntity"));

            if (customerRequestDto.Id.IsNullOrEmpty())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Customer entity Id cannot be empty"));

            BusinessEntity customerEntity = await _businessEntityService.GetByIdAsync(customerRequestDto.Id);
            if (customerEntity == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Customer entity not found for the current user"));

            // Customer
            customerEntity.Name = customerRequestDto.Name;
            customerEntity.Description = customerRequestDto.Description;
            if (customerRequestDto.Address != null && customerRequestDto.Address.Id.IsNotNullOrEmpty())
            {
                Address address = AddressOverviewModelHelper.PrepareAddressEntity(customerRequestDto.Address);
                if (address != null)
                {
                    if (customerEntity.AddressId.IsNotNullOrEmpty())
                    {
                        address.Id = customerEntity.AddressId.Value;
                        await _addressService.Update(address);
                    }
                    else
                    {
                        address.CreatedById = currentUser.Id;
                        address = await _addressService.Insert(address);
                        customerEntity.AddressId = address.Id;
                    }
                }
            }
            await _businessEntityService.UpdateAsync(customerEntity);
            // User
            if (!string.IsNullOrEmpty(customerRequestDto.Email))
                currentUser.Email = await EncryptionHelper.EncryptData(customerRequestDto.Email, currentUser.Salt);
            if (!string.IsNullOrEmpty(customerRequestDto.ContactNo))
                currentUser.ContactNo = await EncryptionHelper.EncryptData(customerRequestDto.ContactNo, currentUser.Salt);
            await _userService.Update(currentUser);

            return NoContent();
        }
        #endregion

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

        //private WebServiceEndpointResponseErrorModel GenerateErrorResponseModel(HttpStatusCode statusCode, string message)
        //{
        //    var errorResponseModel = new WebServiceEndpointResponseErrorModel();
        //    errorResponseModel.Message = message;
        //    switch (statusCode)
        //    {
        //        case HttpStatusCode.OK:
        //            errorResponseModel.Status = HttpStatusCode.OK.ToString();
        //            errorResponseModel.Success = true;
        //            break;

        //        case HttpStatusCode.NotFound:
        //            errorResponseModel.Status = HttpStatusCode.NotFound.ToString();
        //            errorResponseModel.Success = false;
        //            break;

        //        case HttpStatusCode.Unauthorized:
        //            errorResponseModel.Status = HttpStatusCode.Unauthorized.ToString();
        //            errorResponseModel.Success = false;
        //            break;
        //    }
        //    return errorResponseModel;
        //}

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

                // 1. If target type is an interface collection like ICollection<T> or IList<T>
                // we must pass the underlying concrete List<T> type to ConvertValue
                if (targetType.IsGenericType && targetType != typeof(string))
                {
                    var genericDef = targetType.GetGenericTypeDefinition();
                    if (genericDef == typeof(ICollection<>) || genericDef == typeof(IList<>) || genericDef == typeof(IEnumerable<>))
                    {
                        var itemType = targetType.GetGenericArguments()[0];
                        targetType = typeof(List<>).MakeGenericType(itemType);
                    }
                }

                object convertedValue = ConvertValue(kv.Value.ToString(), targetType);

                if (convertedValue != null)
                {
                    // 2. Handle cases where the entity property is already initialized 
                    // and we need to append items instead of overwriting the whole reference
                    var existingValue = prop.GetValue(entity) as IList;
                    if (existingValue != null && !prop.CanWrite && convertedValue is IList incomingList)
                    {
                        foreach (var item in incomingList)
                        {
                            existingValue.Add(item);
                        }
                    }
                    else
                    {
                        // Safe default fallback assignment
                        prop.SetValue(entity, convertedValue);
                    }
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
                // FIX: If the value is a JSON array, let System.Text.Json deserialize it natively
                if (value.TrimStart().StartsWith("["))
                {
                    return System.Text.Json.JsonSerializer.Deserialize(value, targetType, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

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

            if (targetType == typeof(Decimal) &&
                Decimal.TryParse(value, out var dec))
                return dec;

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
