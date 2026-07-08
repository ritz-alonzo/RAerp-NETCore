using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.PluginData.FormTypes
{
    /// <summary>
    /// Form Statuses that will indicate status of every form
    /// </summary>
    public enum FormStatus
    {
        Pending = 1,
        Onhold = 10,
        AwaitingApproval = 20,
        Approved = 30,
        Open = 40,
        Processing = 45,
        AwaitingDelivery = 50,
        Closed = 60,
        Cancelled = 70,
        Deleted = 90
    }
}
