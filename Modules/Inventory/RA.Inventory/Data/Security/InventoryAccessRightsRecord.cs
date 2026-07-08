using RA.Inventory.Helper;
using RAerp.Security.AccessRights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Inventory.Data.Security
{
    public class InventoryAccessRightsRecord
    {
        #region CRUD
        public static AccessRecord ViewInventory = new AccessRecord
        {
            Name = $"View Inventory",
            Classification = "Inventory",
            SystemName = InventoryConstants.InventorySystemName + $".ViewInventory",
            AccessRecordType = AccessType.View
        };
        public static AccessRecord CreateInventory = new AccessRecord
        {
            Name = $"Create Inventory",
            Classification = "Inventory",
            SystemName = InventoryConstants.InventorySystemName + $".CreateInventory",
            AccessRecordType = AccessType.Create
        };
        public static AccessRecord UpdateInventory = new AccessRecord
        {
            Name = $"Update Inventory",
            Classification = "Inventory",
            SystemName = InventoryConstants.InventorySystemName + $".UpdateInventory",
            AccessRecordType = AccessType.Update
        };
        public static AccessRecord DeleteInventory = new AccessRecord
        {
            Name = $"Delete Inventory",
            Classification = "Inventory",
            SystemName = InventoryConstants.InventorySystemName + $".DeleteInventory",
            AccessRecordType = AccessType.Delete
        };
        #endregion
    }
}
