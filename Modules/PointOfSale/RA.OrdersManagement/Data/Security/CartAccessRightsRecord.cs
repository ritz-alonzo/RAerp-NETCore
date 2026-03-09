using RA.OrdersManagement.Domain.Carts;
using RA.OrdersManagement.Domain.Orders;
using RAerp.Security.AccessRights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.Data.Security
{
    public class CartAccessRightsRecord
    {
        #region CRUD
        public static AccessRecord ViewCart = new AccessRecord
        {
            Name = $"View {nameof(Cart)}",
            Classification = nameof(Cart),
            SystemName = typeof(Cart).FullName + $".View{nameof(Cart)}",
            AccessRecordType = AccessType.View
        };
        public static AccessRecord CreateCart = new AccessRecord
        {
            Name = $"Create {nameof(Cart)}",
            Classification = nameof(Cart),
            SystemName = typeof(Cart).FullName + $".Create{nameof(Cart)}",
            AccessRecordType = AccessType.Create
        };
        public static AccessRecord UpdateCart = new AccessRecord
        {
            Name = $"Update {nameof(Cart)}",
            Classification = nameof(Cart),
            SystemName = typeof(Cart).FullName + $".Update{nameof(Cart)}",
            AccessRecordType = AccessType.Update
        };
        public static AccessRecord DeleteCart = new AccessRecord
        {
            Name = $"Delete {nameof(Cart)}",
            Classification = nameof(Cart),
            SystemName = typeof(Cart).FullName + $".Delete{nameof(Cart)}",
            AccessRecordType = AccessType.Delete
        };
        public static AccessRecord ManageCart = new AccessRecord
        {
            Name = $"Manage {nameof(Cart)}",
            Classification = nameof(Cart),
            SystemName = typeof(Cart).FullName + $".Manage{nameof(Cart)}",
            AccessRecordType = AccessType.Manage
        };
        #endregion
    }
}
