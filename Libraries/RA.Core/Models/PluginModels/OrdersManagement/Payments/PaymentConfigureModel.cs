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
        public PaymentConfigureModel()
        {
            AvailableCatalogTypes = new List<SelectListItem>();
        }
        public List<Guid> MappedCatalogTypeIds { get; set; }
        public List<SelectListItem> AvailableCatalogTypes { get; set; }
    }
}
