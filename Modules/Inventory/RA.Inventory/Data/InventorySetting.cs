using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Inventory.Data
{
    public class InventorySetting
    {
        public InventorySetting()
        {
            MappedCatalogTypeIds = new List<Guid>();
        }
        public bool IsEnabled { get; set; }
        public List<Guid> MappedCatalogTypeIds { get; set; }
    }
}
