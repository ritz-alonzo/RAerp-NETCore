using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.DTO.Orders
{
    public class OrderRequestDto : BaseEntity
    {
        public string FormNbr { get; set; }
        public string Description { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? CustomerId { get; set; }
        public DateTime? OrderDate { get; set; }
    }
}
