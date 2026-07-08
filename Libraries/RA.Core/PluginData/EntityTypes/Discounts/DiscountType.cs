using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.PluginData.EntityTypes.Discounts
{
    public enum DiscountType
    {
        Percentage = 10,
        FixedAmount = 20,
        BuyXGetY = 30,
        FreeShipping = 50
    }

    public enum DiscountScope
    {
        OrderLevel = 1,
        ItemLevel = 2,
        CategoryLevel = 3
    }

}
