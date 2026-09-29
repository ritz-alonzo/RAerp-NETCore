using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.DTO.Orders
{
    public class OrderDiscountRequestDto
    {
        public Guid Id { get; set; }
        public Guid DiscountId { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
