using RA.Core.Models.PluginModels.EntityTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Discounts.Models
{
    public class DiscountRedemptionSearchModel : BaseEntitySearchModel
    {
        public string SearchDiscountCode { get; set; }
        public Guid? SearchOrderId { get; set; }
        public string SearchOrderNbr { get; set; }
        public Guid? SearchCustomerId { get; set; }
        public DateTime? SearchRedeemedAt { get; set; }
    }
}
