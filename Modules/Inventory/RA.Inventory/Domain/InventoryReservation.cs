using RA.Core.Domain;
using RA.Core.PluginData.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Inventory.Domain
{
    public class InventoryReservation : BaseEntity
    {
        public Guid InventoryStockId { get;  set; }
        public Guid CatalogId { get;  set; }
        public Guid ReferenceId { get;  set; }
        public string ReferenceNbr { get; set; }
        public Guid WarehouseId { get; set; }
        public int Quantity { get;  set; }
        public int StatusId { get; set; }
        [NotMapped]
        public ReservationStatus Status
        {
            get { return (ReservationStatus)StatusId; }
            set { StatusId = (int)value; }
        }
        public Guid CreatedById { get; set; }
        public DateTime ReservedOn { get;  set; }
        public Guid? ModifiedById { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime? ReleasedOn { get;  set; }
        public DateTime ExpiresOn { get;  set; }
    }
}
