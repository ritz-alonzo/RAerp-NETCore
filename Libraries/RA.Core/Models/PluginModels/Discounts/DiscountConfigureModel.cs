using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Core.Models.PluginModels.EntityTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.Discounts
{
    public class DiscountConfigureModel : BaseEntityConfigureModel
    {
        public DiscountConfigureModel()
        {
            MappedCatalogTypeIds = new List<Guid>();
            AvailableCatalogTypes = new List<SelectListItem>();
        }
        public bool IsApiEnabled { get; set; }
        public List<Guid> MappedCatalogTypeIds { get; set; }
        public List<SelectListItem> AvailableCatalogTypes { get; set; }
    }
}
