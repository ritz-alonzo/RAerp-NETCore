using RA.Core.Models.PluginModels.FormTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.OrdersManagement.Payments
{
    public class PaymentItemModel : BaseFormItemModel
    {
        [DisplayName("Discount")]
        public decimal DiscountAmount { get; set; }
    }
}
