using RA.Core.Models.PluginModels.FormTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.OrdersManagement.Orders
{
    public class OrderItemModel : BaseFormItemModel
    {
        [DisplayName("Discount")]
        public decimal DiscountAmount { get; set; }
    }
}
