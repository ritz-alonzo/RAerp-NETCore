using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Core.Models.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.Inventory
{
    public class InventoryConfigureModel : BaseModel
    {
        public InventoryConfigureModel()
        {
            MappedCatalogTypeIds = new List<Guid>();
            AvailableCatalogTypes = new List<SelectListItem>();
        }
        public string SystemName { get; set; }
        public bool IsEnabled { get; set; }
        public List<Guid> MappedCatalogTypeIds { get; set; }
        public List<SelectListItem> AvailableCatalogTypes { get; set; }
    }
}
