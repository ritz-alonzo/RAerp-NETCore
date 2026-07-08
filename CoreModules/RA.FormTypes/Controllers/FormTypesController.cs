using Microsoft.AspNetCore.Mvc;
using RA.Core.Models.PluginModels.EntityTypes;
using RA.Core.Models.PluginModels.FormTypes;
using RA.FormTypes.Factories;
using RA.FormTypes.Services;
using RAerp.Controllers.Admin;
using RAerp.Security.AccessRightsControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.FormTypes.Controllers
{
    public class FormTypesController : AdminController
    {
        #region Constants
        private readonly IFormTypeModelFactory _formTypeModelFactory;
        private readonly IFormTypeManager _formTypeManager;
        private readonly IAccessControl _accessControl;
        #endregion

        #region Ctor
        public FormTypesController(IFormTypeModelFactory formTypeModelFactory,
            IFormTypeManager formTypeManager,
            IAccessControl accessControl)
        {
            _formTypeModelFactory = formTypeModelFactory;
            _formTypeManager = formTypeManager;
            _accessControl = accessControl;
        }
        #endregion

        public async Task<IActionResult> List(int page = 1)
        {
            if (!(await _accessControl.HasSuperAdminAccessAsync()))
                return UnauthorizedAccess();

            var model = _formTypeModelFactory.PrepareFormTypeSearchModel(new FormTypeSearchModel(), page, 10);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> FormTypeListSearch(FormTypeSearchModel searchModel)
        {
            if (!(await _accessControl.HasSuperAdminAccessAsync()))
                return UnauthorizedAccess();

            var model = _formTypeModelFactory.PrepareFormTypeListModel(searchModel);

            return PartialView(model);
        }
    }
}
