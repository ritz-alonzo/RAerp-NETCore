using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Inventory.Domain
{
    public class InventoryStock : BaseEntity
    {
        // Foreign key to Catalog — no navigation property (cross-module boundary)
        public Guid CatalogId { get; set; }
        public Guid WarehouseId { get;  set; }
        public int QuantityOnHand { get;  set; }
        public int QuantityReserved { get;  set; }
        public int QuantityAvailable { get; set; }
        public int LowStockThreshold { get;  set; }
        public bool IsLowStock { get; set; }
        public Guid CreatedById { get; set; }
        public DateTime CreatedOn { get;  set; }
        public Guid? ModifiedById { get; set; }
        public DateTime? ModifiedOn { get;  set; }
        public bool IsDeleted { get; set; }
    }
}
