using RA.Core.Models.PluginModels.EntityTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Discounts.Models
{
    public class DiscountSearchModel : BaseEntitySearchModel
    {
        public List<int> SearchDiscountTypeIds { get; set; } = new();
        public List<int> SearchDiscountScopeIds { get; set; } = new();
        public bool ShowActiveDiscountsOnly { get; set; }
    }
}
