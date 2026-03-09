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
    public class CartModel : BaseFormModel
    {
        public CartModel()
        {
            AvailableServices = new List<SelectListItem>();
            Items = new CartItemListModel();
        }
        [DisplayName("Service")]
        public Guid? ServiceId { get; set; }
        [DisplayName("Customer Name")]
        public string CustomerName { get; set; }
        [DisplayName("Total Qty")]
        public decimal TotalQty { get; set; }
        [DisplayName("Total Amount")]
        public decimal TotalAmount { get; set; }
        public List<SelectListItem> AvailableServices { get; set; }
        public CartItemListModel Items { get; set; }
    }
}
