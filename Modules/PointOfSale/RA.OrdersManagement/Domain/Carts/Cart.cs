using RA.FormTypes.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.Domain.Carts
{
    public class Cart : BaseForm
    {
        public Guid? ServiceId { get; set; }
        public Guid? OrderId { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
