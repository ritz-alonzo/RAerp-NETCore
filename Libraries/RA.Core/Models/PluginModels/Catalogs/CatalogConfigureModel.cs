using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Core.Models.PluginModels.EntityTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.Catalogs
{
    public class CatalogConfigureModel : BaseEntityConfigureModel
    {
        public CatalogConfigureModel()
        {
            AvailableCategoryTypes = new List<SelectListItem>();
        }
        public bool IsImageEnabled { get; set; }
        public string SKUTemplate { get; set; } // Count will be based on Template Count
        public string BarcodeTemplate { get; set; } // Count will be based on Template Count
        public bool IsSKUEnabled { get; set; }
        public bool IsBarcodeEnabled { get; set; }
        public List<Guid> MappedCategoryTypeIds { get; set; }
        public List<SelectListItem> AvailableCategoryTypes { get; set; }
    }
}
