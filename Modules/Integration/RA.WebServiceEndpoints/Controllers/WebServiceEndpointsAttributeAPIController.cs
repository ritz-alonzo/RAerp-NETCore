using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RA.Core.Domain;
using RA.EntityTypes.Services;
using RA.FormTypes.Helpers;
using RA.WebFramework.Extensions;
using RA.WebFramework.Models.Pagination;
using RA.WebServiceEndpoints.Domain;
using RA.WebServiceEndpoints.Services;
using RAerp.Controllers.Admin;
using RAerp.Domain.Application;
using RAerp.Domain.EntityAttributes;
using RAerp.Domain.EntityTypes;
using RAerp.DTO.EntityAttributes;
using RAerp.Helpers.PluginHelper;
using RAerp.Helpers.UserHelper;
using RAerp.Models.ApiModel;
using RAerp.Security.AccessRightsControl;
using RAerp.Services.ApplicationSettingServices;
using RAerp.Services.EntityAttributeServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RA.WebServiceEndpoints.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/attributes")]
    [Authorize]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class WebServiceEndpointsAttributeAPIController : AdminApiController
    {
        #region Constants
        private readonly IEntityAttributeService _entityAttributeService;
        private readonly IAccessControl _accessControl;
        private readonly IUserIdentity _userIdentity;
        private readonly IApplicationSettingService _applicationSettingService;
        private readonly ApplicationSetting _applicationSetting;
        private readonly IMapper _mapper;
        private readonly IEntityTypeManager _entityTypeManager;
        private readonly IWebServiceEndpointService _webServiceEndpointService;

        public WebServiceEndpointsAttributeAPIController(IEntityAttributeService entityAttributeService,
            IAccessControl accessControl,
            IUserIdentity userIdentity,
            IApplicationSettingService applicationSettingService,
            IMapper mapper,
            IEntityTypeManager entityTypeManager,
            IWebServiceEndpointService webServiceEndpointService)
        {
            _entityAttributeService = entityAttributeService;
            _accessControl = accessControl;
            _userIdentity = userIdentity;
            _applicationSettingService = applicationSettingService;
            _applicationSetting = _applicationSettingService.GetCurrentApplicationSettingAsync().Result;
            _mapper = mapper;
            _entityTypeManager = entityTypeManager;
            _webServiceEndpointService = webServiceEndpointService;
        }
        #endregion

        #region Entity Attributes
        [HttpGet, MapToApiVersion("1.0")]
        public async Task<IActionResult> GetAttributeList([FromQuery] EntityAttributeQueryRequestDto searchRequest)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<EntityAttribute>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (!string.IsNullOrEmpty(searchRequest.WebServiceEndpointName))
            {
                WebServiceEndpoint endpoint = await _webServiceEndpointService.GetEndpointByEndpointName(searchRequest.WebServiceEndpointName);
                if (endpoint != null && endpoint.EndpointEntityTypeId.IsNotNullOrEmpty())
                {
                    EntityType entityType = await _entityTypeManager.GetByIdAsync(endpoint.EndpointEntityTypeId.Value);
                    searchRequest.SearchSystemName = entityType != null ? entityType.EntitySystemName : null;
                }
            }

            if (searchRequest.PageNumber == 0 && searchRequest.PageSize == 0)
            {
                IEnumerable<EntityAttribute> entityAttributeList = await _entityAttributeService.GetEntityAttributeListAsync(
                        searchAttributeName: searchRequest.SearchAttributeName,
                        searchSystemName: searchRequest.SearchSystemName,
                        controlTypeIds: searchRequest.ControlTypeIds,
                        showActiveOnly: searchRequest.ShowActiveOnly
                    );

                IEnumerable<EntityAttributeResponseDto> attributes = _mapper.Map<IEnumerable<EntityAttributeResponseDto>>(entityAttributeList);

                return Ok(GenerateListResponseModel<EntityAttributeResponseDto>(HttpStatusCode.OK, "Successfully get entity attributes", dataList: attributes));
            }
            else
            {
                PagedResult<EntityAttribute> entityAttributeList = await _entityAttributeService.GetEntityAttributePagedResultListAsync(
                        searchAttributeName: searchRequest.SearchAttributeName,
                        searchSystemName: searchRequest.SearchSystemName,
                        controlTypeIds: searchRequest.ControlTypeIds,
                        showActiveOnly: searchRequest.ShowActiveOnly,
                        pageNumber: searchRequest.PageNumber,
                        pageSize: searchRequest.PageSize
                    );

                IEnumerable<EntityAttributeResponseDto> attributes = _mapper.Map<IEnumerable<EntityAttributeResponseDto>>(entityAttributeList.Items);

                return Ok(GenerateListResponseModel<EntityAttributeResponseDto>(HttpStatusCode.OK, "Successfully get entity attributes", pageNumber: searchRequest.PageNumber, pageSize: searchRequest.PageSize, dataList: attributes));
            }
        }

        [HttpPost, MapToApiVersion("1.0")]
        public async Task<IActionResult> CreateAttribute([FromBody] EntityAttributeRequestDto attributeRequest)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<EntityAttribute>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));
            
            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            EntityAttribute attribute = _mapper.Map<EntityAttribute>(attributeRequest);
            attribute.CreatedById = currentUser.Id;

            await _entityAttributeService.InsertEntityAttributeAsync(attribute);

            EntityAttributeResponseDto attributeResponse = _mapper.Map<EntityAttributeResponseDto>(attribute);

            return StatusCode((int)HttpStatusCode.Created, GenerateResponseModel<EntityAttributeResponseDto>(HttpStatusCode.Created, "Successfully created Entity Attribute", attributeResponse));
        }

        [HttpPut, MapToApiVersion("1.0")]
        public async Task<IActionResult> UpdateAttribute([FromBody] EntityAttributeRequestDto attributeRequest)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<EntityAttribute>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            if (attributeRequest.Id.IsNullOrEmpty())
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Id is not provided"));

            EntityAttribute existingAttribute = await _entityAttributeService.GetEntityAttributeByIdAsync(attributeRequest.Id);
            if (existingAttribute == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Entity Attribute doesn't exists"));

            EntityAttribute attribute = _mapper.Map(attributeRequest, existingAttribute);
            attribute.ModifiedById = currentUser.Id;

            await _entityAttributeService.UpdateEntityAttributeAsync(attribute);

            EntityAttributeResponseDto attributeResponse = _mapper.Map<EntityAttributeResponseDto>(attribute);

            return StatusCode((int)HttpStatusCode.OK, GenerateResponseModel<EntityAttributeResponseDto>(HttpStatusCode.Created, "Successfully updated Entity Attribute", attributeResponse));
        }

        [HttpDelete("{id:guid}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> UpdateAttribute(Guid id)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<EntityAttribute>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            if (id.IsNullOrEmpty())
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Id is not provided"));

            EntityAttribute existingAttribute = await _entityAttributeService.GetEntityAttributeByIdAsync(id);
            if (existingAttribute == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Entity Attribute doesn't exists"));

            await _entityAttributeService.DeleteEntityAttributeAsync(existingAttribute);

            EntityAttributeResponseDto attributeResponse = _mapper.Map<EntityAttributeResponseDto>(existingAttribute);

            return StatusCode((int)HttpStatusCode.OK, GenerateResponseModel<EntityAttributeResponseDto>(HttpStatusCode.Created, "Successfully deleted Entity Attribute", attributeResponse));
        }
        #endregion

        [HttpGet("modules"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetEntitySystemNames()
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<EntityAttribute>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            List<EntityAttributeModuleListResponseDto> entities = new List<EntityAttributeModuleListResponseDto>();
            // Entity Types
            var entityTypeList = await _entityTypeManager.GetListAsync(showAllChildEntities: true);
            foreach (var entityType in entityTypeList)
            {
                if (entityType.ParentEntityTypeId.IsNullOrEmpty())
                    continue;

                entities.Add(new EntityAttributeModuleListResponseDto() { SystemName = entityType.EntitySystemName, Name = entityType.EntityName });
            }
            // Independent Entity Types (Discounts)
            var discountEntityType = await _entityTypeManager.GetTypeBySystemNameAsync("RA.Discounts.Domain.Discount");
            if (discountEntityType != null)
                entities.Add(new EntityAttributeModuleListResponseDto() { SystemName = discountEntityType.EntitySystemName, Name = discountEntityType.EntityName });
             // Form Types
            var formTypeList = FormTypeHelper.GetClassesOfBaseForm();
            foreach (var formType in formTypeList)
            {
                entities.Add(new EntityAttributeModuleListResponseDto() { SystemName = formType.FullName, Name = formType.Name });
            }
            //var assemblies = PluginAssemblyHelper.GetAllModulesPluginAssemblies();

            //foreach (var assembly in assemblies)
            //{
            //    if (string.IsNullOrEmpty(assembly?.FullName))
            //        continue;
            //    List<Type> domainTypes = assembly.GetTypes().Where(c => c.IsSubclassOf(typeof(BaseEntity))).ToList();
            //    foreach(var domainType in domainTypes)
            //    {
            //        if (domainType != null && !entities.Any(c => c.SystemName == domainType.FullName))
            //        {
            //            entities.Add(new EntityAttributeModuleListResponseDto() { SystemName = domainType.FullName, Name = domainType.Name });
            //        }
            //    }
            //}

            return Ok(new
            {
                Success = true,
                Data = entities
            });
        }
    }
}
