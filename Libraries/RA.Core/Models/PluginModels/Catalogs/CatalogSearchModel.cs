using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Core.Models.PluginModels.EntityTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.Catalogs
{
    public class CatalogSearchModel : BaseEntitySearchModel
    {
        public CatalogSearchModel()
        {
            Catalogs = new CatalogListModel();
            AvailableCatalogStatus = new List<SelectListItem>();
            SearchCatalogTypeIds = new List<Guid>();
            SearchExistingCatalogIds = new List<Guid>();
        }
        [DisplayName("Status")]
        public int SearchStatusId { get; set; }
        public CatalogListModel Catalogs { get; set; }
        public List<SelectListItem> AvailableCatalogStatus { get; set; }
        public List<Guid> SearchCatalogTypeIds { get; set; }
        public List<Guid> SearchExistingCatalogIds { get; set; }
    }
}
