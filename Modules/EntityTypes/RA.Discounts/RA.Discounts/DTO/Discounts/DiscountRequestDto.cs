using RA.Core.Domain;
using RAerp.Domain.EntityAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Discounts.DTO.Discounts
{
    public class DiscountRequestDto : BaseEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DiscountTypeId { get; set; }
        public int DiscountScopeId { get; set; }
        public decimal Value { get; set; }
        public int? BuyQty { get; set; }
        public int? GetQty { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public int? MinQty { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int? UsageLimitTotal { get; set; }
        public int? UsageLimitPerCustomer { get; set; }
        public int UsageCount { get; set; }
        public bool IsActive { get; set; }
        public List<EntityAttributeValue> Attributes { get; set; } = new List<EntityAttributeValue>();
    }
}
