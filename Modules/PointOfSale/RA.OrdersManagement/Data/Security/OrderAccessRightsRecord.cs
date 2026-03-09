using RA.OrdersManagement.Domain.Orders;
using RAerp.Security.AccessRights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.Data.Security
{
    public class OrderAccessRightsRecord
    {
        #region CRUD
        public static AccessRecord ViewOrder = new AccessRecord
        {
            Name = $"View {nameof(Order)}",
            Classification = nameof(Order),
            SystemName = typeof(Order).FullName + $".View{nameof(Order)}",
            AccessRecordType = AccessType.View
        };
        public static AccessRecord CreateOrder = new AccessRecord
        {
            Name = $"Create {nameof(Order)}",
            Classification = nameof(Order),
            SystemName = typeof(Order).FullName + $".Create{nameof(Order)}",
            AccessRecordType = AccessType.Create
        };
        public static AccessRecord UpdateOrder = new AccessRecord
        {
            Name = $"Update {nameof(Order)}",
            Classification = nameof(Order),
            SystemName = typeof(Order).FullName + $".Update{nameof(Order)}",
            AccessRecordType = AccessType.Update
        };
        public static AccessRecord DeleteOrder = new AccessRecord
        {
            Name = $"Delete {nameof(Order)}",
            Classification = nameof(Order),
            SystemName = typeof(Order).FullName + $".Delete{nameof(Order)}",
            AccessRecordType = AccessType.Delete
        };
        public static AccessRecord ManageOrder = new AccessRecord
        {
            Name = $"Manage {nameof(Order)}",
            Classification = nameof(Order),
            SystemName = typeof(Order).FullName + $".Manage{nameof(Order)}",
            AccessRecordType = AccessType.Manage
        };
        #endregion
    }
}
