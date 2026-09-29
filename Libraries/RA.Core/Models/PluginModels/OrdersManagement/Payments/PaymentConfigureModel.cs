using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Core.Models.PluginModels.FormTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.OrdersManagement.Payments
{
    public class PaymentConfigureModel : BaseFormConfigureModel
    {
        public List<Guid> MappedCatalogTypeIds { get; set; }
        public List<Guid> MappedCategoryTypeIds { get; set; }
        public List<SelectListItem> AvailableCatalogTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> AvailableCategoryTypes { get; set; } = new List<SelectListItem>();
    }
}
