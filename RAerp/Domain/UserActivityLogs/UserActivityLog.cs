using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAerp.Domain.UserActivityLogs
{
    public class UserActivityLog
    {
        public Guid Id { get; set; }
        public string Data { get; set; }
        public string LastVisitedUrl { get; set; }
        public string UserName { get; set; }
        public Guid? UserId { get; set; }
        public DateTime TimeStamp { get; set; }
        public string UserIpAddress { get; set; }
    }
}
