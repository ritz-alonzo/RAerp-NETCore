using Microsoft.AspNetCore.Mvc;
using RA.Core.Models.PluginModels.Catalogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Catalogs.Components
{
    /// <summary>
    /// Catalogs API Mapping
    /// </summary>
    public class CatalogApiViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new CatalogModel();
            return View("/Views/Components/CatalogApiMapping.cshtml", model);
        }
    }
}
