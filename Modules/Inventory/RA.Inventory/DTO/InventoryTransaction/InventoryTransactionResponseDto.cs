using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Inventory.DTO.InventoryTransaction
{
    public class InventoryTransactionResponseDto : BaseEntity
    {
        public Guid InventoryStockId { get; set; }
        public Guid CatalogId { get; set; }
        public Guid WarehouseId { get; set; }
        public int TransactionTypeId { get; set; }
        public int Quantity { get; set; }
        public int StockAfter { get; set; }
        public string ReferenceNbr { get; set; }
        public string Note { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
