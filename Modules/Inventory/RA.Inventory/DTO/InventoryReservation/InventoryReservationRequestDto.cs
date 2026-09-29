using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Inventory.DTO.InventoryReservation
{
    public class InventoryReservationRequestDto : BaseEntity
    {
        public Guid CatalogId { get; set; }
        public Guid ReferenceId { get; set; }
        public string ReferenceNbr { get; set; }
        public Guid WarehouseId { get; set; }
        public int Quantity { get; set; }
    }
}
