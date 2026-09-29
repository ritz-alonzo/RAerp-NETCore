using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.DTO.Orders
{
    public class OrderResponseDto : BaseEntity
    {
        public string FormNbr { get; set; }
        public int StatusId { get; set; }
        public string Description { get; set; }
        public Guid? CartId { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalQty { get; set; }
        public decimal TotalDiscountAmount { get; set; }
        public decimal TotalVatAmount { get; set; }
        public decimal TotalGrossAmount { get; set; }
        public decimal TotalNetAmount { get; set; }
    }
}
