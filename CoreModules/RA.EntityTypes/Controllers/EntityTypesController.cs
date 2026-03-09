using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using RA.Core.Models.PluginModels.EntityTypes;
using RA.Data.Domain.EntityTypes;
using RA.Data.Domain.Users;
using RA.EntityTypes.Factories;
using RA.EntityTypes.Helpers;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;
using RAerp.Controllers.Admin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RA.EntityTypes.Controllers
{
    public class EntityTypesController : AdminController
    {
        private readonly IEntityTypeModelFactory _entityTypeModelFactory;
        private readonly IEntityTypeManager _entityTypeManager;

        public EntityTypesController(IEntityTypeModelFactory entityTypeModelFactory, IEntityTypeManager entityTypeManager)
        {
            _entityTypeModelFactory = entityTypeModelFactory;
            _entityTypeManager = entityTypeManager;
        }

        public async Task<IActionResult> List(int page = 1)
        {
            // default page size
            int pageSize = 1;
            var model = await _entityTypeModelFactory.PrepareEntityTypeSearchModelAsync(new EntityTypeSearchModel(), page, pageSize);

            return View("~/Plugins/RA.EntityTypes/Views/List.cshtml", model);
        }

        // working table filter
        // will need to update preparing Plugin list
        [HttpGet]
        public async Task<IActionResult> EntityTypeListSearch(EntityTypeSearchModel searchModel)
        {
            var model = await _entityTypeModelFactory.PrepareEntityTypeListModelAsync(searchModel);

            return PartialView("~/Plugins/RA.EntityTypes/Views/_EntityTypeList.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> ChildEntityTypeList(Guid id, int page = 1)
        {
            if (id.IsNullOrEmpty())
                throw new Exception(EntityTypeMessages.ParentEntityTypeIdNotExists);
            // default page size
            int pageSize = 10;

            var entityTypeSearchModel = new EntityTypeSearchModel();

            entityTypeSearchModel.SearchParentEntityTypeId = id;
            entityTypeSearchModel.ChildEntitySearchEnabled = true;

            var model = await _entityTypeModelFactory.PrepareEntityTypeSearchModelAsync(entityTypeSearchModel, page, pageSize);

            return View("~/Plugins/RA.EntityTypes/Views/ChildEntityTypeList.cshtml", model);
        }

        // working table filter
        // will need to update preparing Plugin list
        [HttpGet]
        public async Task<IActionResult> ChildEntityTypeListSearch(EntityTypeSearchModel searchModel)
        {
            var model = await _entityTypeModelFactory.PrepareChildEntityTypeListModelAsync(searchModel);

            return PartialView("~/Plugins/RA.EntityTypes/Views/_ChildEntityTypeListSearch.cshtml", model);
        }

        // Modal creation for Adding child entity
        [HttpGet]
        public async Task<IActionResult> CreateChildEntity(Guid parentEntityTypeId)
        {
            if (parentEntityTypeId.IsNullOrEmpty())
                throw new Exception(EntityTypeMessages.ParentEntityTypeIdNotExists);

            var model = await _entityTypeModelFactory.PrepareEntityTypeModelAsync(new EntityTypeModel(), parentEntityTypeId, true);

            return View("~/Plugins/RA.EntityTypes/Views/CreateChildEntity.cshtml", model);
        }


        [HttpPost]
        public async Task<IActionResult> CreateChildEntity(EntityTypeModel model)
        {
            if (model.ParentEntityTypeId.IsNullOrEmpty())
                return JsonError(EntityTypeMessages.ParentEntityTypeIdNotExists);

            var parentEntityType = await _entityTypeManager.GetByIdAsync(model.ParentEntityTypeId.Value);
            if (parentEntityType == null)
                return JsonError(EntityTypeMessages.ParentTypeNotExists);

            if (ModelState.IsValid)
            {
                var childEntityType = new EntityType()
                {
                    EntityName = model.EntityName,
                    EntitySystemName = parentEntityType.EntitySystemName + $".{model.EntityName}",
                    Installed = true,
                    ParentEntityTypeId = model.ParentEntityTypeId
                };
                await _entityTypeManager.InsertAsync(childEntityType);
            }
            else
            {
                var errorMessage = ModelJsonValidationErrorMessages(ModelState);
                return JsonError(errorMessage);
            }

            return NullJsonResult();
        }
    }
}
