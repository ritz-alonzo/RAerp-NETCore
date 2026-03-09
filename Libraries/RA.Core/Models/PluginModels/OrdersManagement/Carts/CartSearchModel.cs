using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Core.Models.PluginModels.FormTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.OrdersManagement.Carts
{
    public class CartSearchModel : BaseFormSearchModel
    {
        public CartSearchModel()
        {
            Items = new CartListModel();
            AvailableServiceTypes = new List<SelectListItem>();
        }
        [DisplayName("Service")]
        public Guid? SearchServiceId { get; set; }
        [DisplayName("Customer Name")]
        public string SearchCustomerName { get; set; }
        public List<SelectListItem> AvailableServiceTypes { get; set; }
        public CartListModel Items { get; set; }
    }
}
