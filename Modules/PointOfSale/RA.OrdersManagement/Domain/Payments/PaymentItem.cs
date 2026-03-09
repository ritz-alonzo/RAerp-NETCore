using RA.FormTypes.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.Domain.Payments
{
    public class PaymentItem : BaseFormItem
    {
        public decimal DiscountAmount { get; set; }
    }
}
