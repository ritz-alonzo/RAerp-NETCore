using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RA.BusinessEntities.Data;
using RA.BusinessEntities.Domain;
using RA.BusinessEntities.Services;
using RA.Catalogs.Data;
using RA.Catalogs.Domain;
using RA.Catalogs.Services;
using RA.Categories.Data;
using RA.Categories.Domain;
using RA.Categories.Services;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;
using RA.WebServiceEndpoints.Domain;
using RA.WebServiceEndpoints.Services;
using RAerp.Controllers.Admin;
using RAerp.Domain.FileManager;
using RAerp.DTO.FileMapping;
using RAerp.Helpers.UserHelper;
using RAerp.Services.FileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RA.WebServiceEndpoints.Controllers
{
    [ApiController]
    [Route("api/uploads")]
    [Authorize]
    public class WebServiceEndpointsFileAPIController : AdminApiController
    {
        #region Constants
        private readonly IFileService _fileService;
        private readonly IUserIdentity _userIdentity;
        private readonly IEntityTypeManager _entityTypeManager;
        private readonly IWebServiceEndpointService _webServiceEndpointService;
        private readonly IBusinessEntityService _businessEntityService;
        private readonly ICategoryService _categoryService;
        private readonly ICatalogService _catalogService;

        public WebServiceEndpointsFileAPIController(IFileService fileService,
            IUserIdentity userIdentity,
            IEntityTypeManager entityTypeManager,
            IWebServiceEndpointService webServiceEndpointService,
            IBusinessEntityService businessEntityService,
            ICategoryService categoryService,
            ICatalogService catalogService)
        {
            _fileService = fileService;
            _userIdentity = userIdentity;
            _entityTypeManager = entityTypeManager;
            _webServiceEndpointService = webServiceEndpointService;
            _businessEntityService = businessEntityService;
            _categoryService = categoryService;
            _catalogService = catalogService;
        }
        #endregion

        [HttpGet]
        public async Task<IActionResult> GetFiles([FromQuery] FileMappingQueryRequestDto query)
        {
            var currentApiUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);
            if (currentApiUser == null) 
                return NotFound("User not found.");

            var files = await _fileService.GetListAsync();
            return Ok(files);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile([FromForm] FileMappingRequestDto fileMapping)
        {
            if (fileMapping.File == null || fileMapping.File.Length == 0)
                return BadRequest("No file uploaded.");

            var currentApiUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);
            if (currentApiUser == null)
                return NotFound("User not found.");

            if (fileMapping.EntityId.IsNullOrEmpty())
                return BadRequest("Entity Id should not be empty");

            if (string.IsNullOrEmpty(fileMapping.SystemName))
                return BadRequest("System name should not be empty");

            try
            {
                // Check if the file already exists for the given entity and system name
                FileEntityMapping existingFileMapping = await _fileService.GetByEntityIdAsync(fileMapping.EntityId);
                if (existingFileMapping == null)
                {
                    existingFileMapping = await _fileService.CreateAsync(fileMapping.File, fileMapping.EntityId, fileMapping.SystemName);
                }
                else
                {
                    existingFileMapping.FilePath = await _fileService.SaveFileAsync(fileMapping.File);
                    existingFileMapping.FileName = fileMapping.File.FileName;
                    existingFileMapping.FileType = fileMapping.File.ContentType;
                    existingFileMapping.FileSize = fileMapping.File.Length;
                    existingFileMapping.CreatedAt = DateTime.UtcNow;
                    existingFileMapping = await _fileService.UpdateAsync(existingFileMapping);
                }

                // Entity Types with WebService Endpoints configured
                if (!string.IsNullOrEmpty(fileMapping.Endpoint))
                {
                    WebServiceEndpoint webServiceEndpoint = await _webServiceEndpointService.GetEndpointByEndpointName(fileMapping.Endpoint);
                    if (webServiceEndpoint != null)
                    {
                        switch (webServiceEndpoint.EndpointDomain)
                        {
                            case nameof(BusinessEntity):
                                BusinessEntity businessEntity = await _businessEntityService.GetByIdAsync(fileMapping.EntityId);
                                if (businessEntity == null)
                                    return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Business entity cannot be found"));
                                BusinessEntitySetting businessEntitySetting = await _entityTypeManager.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(webServiceEndpoint.EndpointEntityTypeId.Value);
                                if (businessEntitySetting == null)
                                    return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Business entity not yet configured"));
                                //if (businessEntitySetting.)
                                //// Check if File upload in 
                                //await _fileService.UpdateFileMappingAsync(fileEntity, fileMapping.FilePath);
                                break;
                            case nameof(Category):
                                Category category = await _categoryService.GetByIdAsync(fileMapping.EntityId);
                                if (category == null)
                                    return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Category cannot be found"));
                                CategorySetting categorySetting = await _entityTypeManager.GetSettingDataOfEntityAsync<Category, CategorySetting>(webServiceEndpoint.EndpointEntityTypeId.Value);
                                if (categorySetting == null)
                                    return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Category not yet configured"));
                                if (categorySetting.IsImageEnabled)
                                {
                                    category.ImagePath = existingFileMapping.FilePath;
                                    await _categoryService.UpdateAsync(category);
                                }
                                break;
                            case nameof(Catalog):
                                Catalog catalog = await _catalogService.GetByIdAsync(fileMapping.EntityId);
                                if (catalog == null)
                                    return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Catalog cannot be found"));
                                CatalogSetting catalogSetting = await _entityTypeManager.GetSettingDataOfEntityAsync<Catalog, CatalogSetting>(webServiceEndpoint.EndpointEntityTypeId.Value);
                                if (catalog == null)
                                    return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Catalog not yet configured"));
                                if (catalogSetting.IsImageEnabled)
                                {
                                    catalog.ImagePath = existingFileMapping.FilePath;
                                    await _catalogService.UpdateAsync(catalog);
                                }
                                break;
                            default:
                                return BadRequest("Unsupported entity type for file mapping.");
                        }
                    }
                }
                // Form or Independent Entity Type
                else
                {

                }

                return Ok(existingFileMapping);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
