using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Discounts.DTO.DiscountRedemptions
{
    public class DiscountRedemptionRequestDto : BaseEntity
    {
        public Guid DiscountId { get; set; }
        public string DiscountCode { get; set; }
        public Guid OrderId { get; set; }
        public string OrderNbr { get; set; }
        public Guid CustomerId { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountedAmount { get; set; }
        public DateTime RedeemedAt { get; set; }
    }
}
