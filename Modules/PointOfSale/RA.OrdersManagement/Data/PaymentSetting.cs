using RA.FormTypes.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.Data
{
    public class PaymentSetting : BaseFormSetting
    {
        public PaymentSetting()
        {
            MappedCatalogTypeIds = new List<Guid>();
        }
        public List<Guid> MappedCatalogTypeIds { get; set; }
    }
}
