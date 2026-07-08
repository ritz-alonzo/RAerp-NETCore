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
        public List<int> DiscountTypeIds { get; set; } = new();
        public List<int> DiscountScopeIds { get; set; } = new();
        public bool ShowActiveDiscountsOnly { get; set; }
    }
}
