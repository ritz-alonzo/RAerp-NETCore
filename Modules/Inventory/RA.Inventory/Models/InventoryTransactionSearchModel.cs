using RA.Core.Models.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Inventory.Models
{
    public class InventoryTransactionSearchModel : BaseSearchModel
    {
        public Guid? StockId { get; set; }
        public Guid? CatalogTypeId { get; set; }
        public List<Guid> CatalogIds { get; set; } = new();
        public List<Guid> WarehouseIds { get; set; } = new();
        public Guid? ReferenceId { get; set; }
        public string ReferenceNbr { get; set; }
        public List<int> TransactionTypeIds { get; set; } = new();
        public DateTime? CreatedOn { get; set; }
        public bool ShowLowStockOnly { get; set; } = false;
        public bool SortByCatalogName { get; set; } = false;
    }
}
