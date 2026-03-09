using RA.OrdersManagement.Domain.Orders;
using RA.OrdersManagement.Domain.Payments;
using RAerp.Security.AccessRights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.Data.Security
{
    public class PaymentAccessRightsRecord
    {
        #region CRUD
        public static AccessRecord ViewPayment = new AccessRecord
        {
            Name = $"View {nameof(Payment)}",
            Classification = nameof(Payment),
            SystemName = typeof(Payment).FullName + $".View{nameof(Payment)}",
            AccessRecordType = AccessType.View
        };
        public static AccessRecord CreatePayment = new AccessRecord
        {
            Name = $"Create {nameof(Payment)}",
            Classification = nameof(Payment),
            SystemName = typeof(Payment).FullName + $".Create{nameof(Payment)}",
            AccessRecordType = AccessType.Create
        };
        public static AccessRecord UpdatePayment = new AccessRecord
        {
            Name = $"Update {nameof(Payment)}",
            Classification = nameof(Payment),
            SystemName = typeof(Payment).FullName + $".Update{nameof(Payment)}",
            AccessRecordType = AccessType.Update
        };
        public static AccessRecord DeletePayment = new AccessRecord
        {
            Name = $"Delete {nameof(Payment)}",
            Classification = nameof(Payment),
            SystemName = typeof(Payment).FullName + $".Delete{nameof(Payment)}",
            AccessRecordType = AccessType.Delete
        };
        public static AccessRecord ManagePayment = new AccessRecord
        {
            Name = $"Manage {nameof(Payment)}",
            Classification = nameof(Payment),
            SystemName = typeof(Payment).FullName + $".Manage{nameof(Payment)}",
            AccessRecordType = AccessType.Manage
        };
        #endregion
    }
}
