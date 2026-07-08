using RA.EntityTypes.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Discounts.Data
{
    public class DiscountSetting : BaseEntityTypeSetting
    {
        public DiscountSetting()
        {
            MappedCatalogIds = new List<Guid>();
        }
        public bool IsApiEnabled { get; set; }
        public List<Guid> MappedCatalogIds { get; set; }
    }
}
