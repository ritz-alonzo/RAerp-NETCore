using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Core.Models.PluginModels.FormTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.OrdersManagement.Orders
{
    public class OrderSearchModel : BaseFormSearchModel
    {
        public OrderSearchModel()
        {
            Items = new OrderListModel();
            AvailableServiceTypes = new List<SelectListItem>();
        }
        [DisplayName("Service")]
        public Guid? SearchServiceId { get; set; }
        [DisplayName("Customer Name")]
        public string SearchCustomerName { get; set; }
        [DisplayName("Order Date")]
        public DateTime? SearchOrderDate { get; set; }
        public List<SelectListItem> AvailableServiceTypes { get; set; }
        public OrderListModel Items { get; set; }
    }
}
