using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.DTO.Orders
{
    public class OrderItemRequestDto : BaseEntity
    {
        public Guid FormId { get; set; }
        public Guid CatalogId { get; set; }
        public int LineNbr { get; set; }
        public Guid? CategoryId { get; set; }
        public string Description { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
