using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Data.Data.FormTypes
{
    /// <summary>
    /// Form Statuses that will indicate status of every form
    /// </summary>
    public enum FormStatus
    {
        Pending = 0,
        Onhold = 10,
        Ongoing = 20,
        AwaitingApproval = 30,
        Approved = 40,
        Closed = 50,
        Deleted = 90
    }
}
