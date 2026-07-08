using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.PluginData.Inventory
{
    public enum TransactionType
    {
        NewlyAdded = 1,
        PurchaseOrder = 10, // Purchase Order
        SalesOrder = 20, // Sales Order
        PurchaseReturn = 30, // Purchase Return
        SalesReturn = 40, // Sales Return
        Adjustment = 50, // Stock Adjustment
        Transfer = 60, // Stock Transfer
        Reserve = 70 // Stock Reserve
    }
}
