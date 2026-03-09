using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Core.Models.PluginModels.FormTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.OrdersManagement.Orders
{
    public class OrderModel : BaseFormModel
    {
        public OrderModel()
        {
            AvailableServices = new List<SelectListItem>();
            Items = new OrderItemListModel();
        }
        [DisplayName("Service")]
        public Guid? ServiceId { get; set; }
        [DisplayName("Customer Name")]
        public string CustomerName { get; set; }
        [DisplayName("Order Date")]
        [DataType(DataType.DateTime)]
        public DateTime OrderDate { get; set; }
        [DisplayName("Total Qty")]
        public decimal TotalQty { get; set; }
        [DisplayName("Total Discount Amount")]
        public decimal TotalDiscountAmount { get; set; }
        [DisplayName("VAT Amount")]
        public decimal TotalVatAmount { get; set; }
        [DisplayName("Total Gross Amount")]
        public decimal TotalGrossAmount { get; set; }
        [DisplayName("Total Amount")]
        public decimal TotalNetAmount { get; set; }
        public List<SelectListItem> AvailableServices { get; set; }
        public OrderItemListModel Items { get; set; }
    }
}
