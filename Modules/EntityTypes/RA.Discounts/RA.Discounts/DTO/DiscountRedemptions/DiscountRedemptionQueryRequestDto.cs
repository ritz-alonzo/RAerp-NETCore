using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Discounts.DTO.DiscountRedemptions
{
    public class DiscountRedemptionQueryRequestDto
    {
        public string SearchDiscountCode { get; set; }
        public Guid? SearchOrderId { get; set; }
        public string SearchOrderNbr { get; set; }
        public Guid? SearchCustomerId { get; set; }
        public DateTime? SearchRedeemedAt { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}
