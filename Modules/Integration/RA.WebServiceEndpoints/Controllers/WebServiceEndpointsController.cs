using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RA.Core.Models.PluginModels.BusinessEntities;
using RA.Core.Models.PluginModels.EntityTypes;
using RA.WebFramework.Extensions;
using RA.WebServiceEndpoints.Domain;
using RA.WebServiceEndpoints.Factories;
using RA.Core.Models.PluginModels.WebServiceEndpoints;
using RA.WebServiceEndpoints.Services;
using RAerp.Controllers.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RA.EntityTypes.Services;
using RAerp.Helpers.UserHelper;

namespace RA.WebServiceEndpoints.Controllers
{
    public class WebServiceEndpointsController : AdminController
    {
        #region Constants
        private readonly IWebServiceEndpointService _webServiceEndpointService;
        private readonly IWebServiceEndpointModelFactory _webServiceEndpointModelFactory;
        private readonly IMapper _mapper;
        private readonly IEntityTypeManager _entityTypeManager;
        private readonly IUserIdentity _userIdentity;
        #endregion

        #region Ctor
        public WebServiceEndpointsController(IWebServiceEndpointService webServiceEndpointService,
            IWebServiceEndpointModelFactory webServiceEndpointModelFactory,
            IMapper mapper,
            IEntityTypeManager entityTypeManager,
            IUserIdentity userIdentity)
        {
            _webServiceEndpointService = webServiceEndpointService;
            _webServiceEndpointModelFactory = webServiceEndpointModelFactory;
            _mapper = mapper;
            _entityTypeManager = entityTypeManager;
            _userIdentity = userIdentity;
        }
        #endregion

        #region Configuration

        #endregion

        #region CRUD

        public async Task<IActionResult> List(int page = 1)
        {
            // default page size
            int pageSize = 10;
            var model = await _webServiceEndpointModelFactory.PrepareWebServiceEndpointSearchModelAsync(new WebServiceEndpointSearchModel(), page, pageSize);

            return View(model);
        }

        // working table filter
        // will need to update preparing Plugin list
        [HttpGet]
        public async Task<IActionResult> WebServiceEndpointListSearch(WebServiceEndpointSearchModel searchModel)
        {
            var model = await _webServiceEndpointModelFactory.PrepareWebServiceEndpointListModelAsync(searchModel);

            return PartialView(model);
        }

        public async Task<IActionResult> Index(Guid id)
        {
            if (id.IsNullOrEmpty())
                return NotFound();

            var entity = await _webServiceEndpointService.GetById(id);
            if (entity == null)
                return NotFound();

            var model = await _webServiceEndpointModelFactory.PrepareWebServiceEndpointModelAsync(new WebServiceEndpointModel(), entity);

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            //if (entityTypeId.IsNullOrEmpty())
            //    return NotFound();

            //var entityTypeSetting = await _entityTypeManager.GetSettingDataOfEntity<BusinessEntity, BusinessEntitySetting>(entityTypeId);
            //if (entityTypeSetting == null)
            //    return NotFound();

            //if (!entityTypeSetting.Enabled)
            //    return NotFound();

            var model = await _webServiceEndpointModelFactory.PrepareWebServiceEndpointModelAsync(new WebServiceEndpointModel(), null);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WebServiceEndpointModel model)
        {
            if (ModelState.IsValid)
            {
                var entity = _mapper.Map<WebServiceEndpoint>(model);
                entity.CreatedById = (await _userIdentity.GetCurrentUserAsync(HttpContext)).Id;

                // Save Domain name
                if (entity.EndpointEntityTypeId.IsNotNullOrEmpty())
                {
                    var parentEntityType = await _entityTypeManager.GetParentEntityTypeByChildEntityTypeIdAsync(entity.EndpointEntityTypeId.Value);
                    entity.EndpointDomain = parentEntityType.EntityName;
                }

                await _webServiceEndpointService.Insert(entity);
                // sanity check
                model.Id = entity.Id;

                SuccessNotification(model, "Successfully created Web Service Endpoint");
            }
            else
            {
                ErrorNotification(model, "Failed to create Web Service Endpoint");
                return View("Create");
            }

            return RedirectToAction("Index", new { id = model.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(WebServiceEndpointModel model)
        {
            if (ModelState.IsValid)
            {
                var entity = _mapper.Map<WebServiceEndpoint>(model);
                entity.ModifiedById = (await _userIdentity.GetCurrentUserAsync(HttpContext)).Id;
                await _webServiceEndpointService.Update(entity);
                // sanity check
                model.Id = entity.Id;
                SuccessNotification(model, "Successfully created Web Service Endpoint");
            }
            else
            {
                ErrorNotification(model, "Failed to update Web Service Endpoint");
                return View("Index", new { id = model.Id });
            }

            return RedirectToAction("Index", new { id = model.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id.IsNullOrEmpty())
                return NotFound();

            var entity = await _webServiceEndpointService.GetById(id);
            if (entity == null)
                return NotFound();

            await _webServiceEndpointService.Delete(entity);
            SuccessNotification(new BusinessEntityModel() { Id = entity.Id }, "Successfully deleted Web Service Endpoint");

            return RedirectToAction("List");
        }

        #endregion

    }
}
