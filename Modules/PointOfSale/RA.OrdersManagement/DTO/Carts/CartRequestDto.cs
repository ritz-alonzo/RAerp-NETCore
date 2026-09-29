using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.DTO.Carts
{
    public class CartRequestDto : BaseEntity
    {
        public string FormNbr { get; set; }
        public string Description { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
