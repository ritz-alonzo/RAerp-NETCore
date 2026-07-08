using Microsoft.AspNetCore.Mvc;
using RA.BusinessEntities.Domain;
using RA.BusinessEntities.Factories;
using RA.Categories.Domain;
using RA.Core.Models.PluginModels.BusinessEntities;
using RA.Core.Models.PluginModels.Categories;
using RA.Core.Models.PortableViewModels;
using RAerp.PluginServiceProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.BusinessEntities.Components
{
    public class BusinessEntityTestViewComponent : ViewComponent
    {
        private readonly IBusinessEntityModelFactory _businessEntityModelFactory;

        public BusinessEntityTestViewComponent(IBusinessEntityModelFactory businessEntityModelFactory)
        {
            _businessEntityModelFactory = businessEntityModelFactory;
        }

        public IViewComponentResult Invoke(CategoryModel categoryModel)
        {
            var model = new BusinessEntityModel();
            return View("/Views/Components/_BusinessEntityTest.cshtml", model);
        }
    }
}
