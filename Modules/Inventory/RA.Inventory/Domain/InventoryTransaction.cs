using RA.Core.Domain;
using RA.Core.PluginData.EntityTypes.Discounts;
using RA.Core.PluginData.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Inventory.Domain
{
    public class InventoryTransaction : BaseEntity
    {
        public Guid InventoryStockId { get; set; }
        public Guid CatalogId { get; set; }
        public Guid WarehouseId { get; set; }
        public int TransactionTypeId { get; set; }
        [NotMapped]
        public TransactionType TransactionType
        {
            get { return (TransactionType)TransactionTypeId; }
            set { TransactionTypeId = (int)value; }
        }
        public int Quantity { get; set; }
        public int StockAfter { get; set; } 
        public Guid? ReferenceId { get; set; }  // OrderId, SalesOrderId, PurchaseOrderId
        public string ReferenceNbr { get; set; }
        public string Note { get; set; }
        public Guid CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid? ModifiedById { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
