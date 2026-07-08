using RA.Core.PluginData.EntityTypes.Discounts;
using RA.EntityTypes.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Discounts.Domain
{
    public class Discount : BaseEntityType
    {
        public int DiscountTypeId { get; set; }
        [NotMapped]
        public DiscountType DiscountType
        {
            get { return (DiscountType)DiscountTypeId; }
            set { DiscountTypeId = (int)value; }
        }
        public int DiscountScopeId { get; set; }
        [NotMapped]
        public DiscountScope DiscountScope
        {
            get { return (DiscountScope)DiscountScopeId; }
            set { DiscountScopeId = (int)value; }
        }
        public decimal Value { get; set; }
        // For BuyXGeyY Discount Type
        public int? BuyQty { get; set; }
        public int? GetQty { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public int? MinQty { get; set; }
        public string EligibleCatalogIds { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int? UsageLimitTotal { get; set; }
        public int? UsageLimitPerCustomer { get; set; }
        public int UsageCount { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
