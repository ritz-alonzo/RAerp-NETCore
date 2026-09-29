using RA.Inventory.Domain;
using RA.Inventory.Helper;
using RAerp.Security.AccessRights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Inventory.Data.Security
{
    public class InventoryStockAccessRightsRecord
    {
        #region CRUD
        public static AccessRecord ViewInventory = new AccessRecord
        {
            Name = $"View {nameof(InventoryStock)}",
            Classification = "Inventory",
            SystemName = InventoryConstants.InventorySystemName + $".ViewInventory",
            AccessRecordType = AccessType.View
        };
        public static AccessRecord CreateInventory = new AccessRecord
        {
            Name = $"Create {nameof(InventoryStock)}",
            Classification = "Inventory",
            SystemName = InventoryConstants.InventorySystemName + $".CreateInventory",
            AccessRecordType = AccessType.Create
        };
        public static AccessRecord UpdateInventory = new AccessRecord
        {
            Name = $"Update {nameof(InventoryStock)}",
            Classification = "Inventory",
            SystemName = InventoryConstants.InventorySystemName + $".UpdateInventory",
            AccessRecordType = AccessType.Update
        };
        public static AccessRecord DeleteInventory = new AccessRecord
        {
            Name = $"Delete {nameof(InventoryStock)}",
            Classification = "Inventory",
            SystemName = InventoryConstants.InventorySystemName + $".DeleteInventory",
            AccessRecordType = AccessType.Delete
        };
        #endregion
    }
}
