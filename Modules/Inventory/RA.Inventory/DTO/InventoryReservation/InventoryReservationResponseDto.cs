using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Inventory.DTO.InventoryReservation
{
    public class InventoryReservationResponseDto : BaseEntity
    {
        public Guid CatalogId { get; set; }
        public Guid ReferenceId { get; set; }
        public string ReferenceNbr { get; set; }
        public Guid WarehouseId { get; set; }
        public int Quantity { get; set; }
        public int StatusId { get; set; }
        public DateTime ReservedOn { get; set; }
        public DateTime ExpiresOn { get; set; }
        public DateTime? ReleasedOn { get; set; }
    }
}
