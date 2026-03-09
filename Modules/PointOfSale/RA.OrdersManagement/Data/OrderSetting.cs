using RA.FormTypes.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.Data
{
    public class OrderSetting : BaseFormSetting
    {
        public OrderSetting()
        {
            MappedCatalogTypeIds = new List<Guid>();
            MappedServiceTypeIds = new List<Guid>();
        }
        public List<Guid> MappedCatalogTypeIds { get; set; }
        public List<Guid> MappedServiceTypeIds { get; set; }
    }
}
