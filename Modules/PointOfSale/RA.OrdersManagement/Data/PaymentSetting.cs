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
        public List<Guid> MappedCatalogTypeIds { get; set; } = new List<Guid>();
        public List<Guid> MappedCategoryTypeIds { get; set; } = new List<Guid>();
    }
}
