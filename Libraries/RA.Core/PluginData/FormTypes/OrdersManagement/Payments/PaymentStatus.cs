using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.PluginData.FormTypes.OrdersManagement.Payments
{
    public enum PaymentStatus
    {
        Pending = 1,
        AwaitingPayment = 10,
        Completed = 20,
        Voided = 30
    }
}
