using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Domain
{
    /// <summary>
    /// This base class is for admin 
    /// users, logs, etc. 
    /// only admin entities will be able to use this class
    /// </summary>
    public class BaseAdminEntity
    {
        public Guid Id { get; set; }
        public Guid? CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid? ModifiedById { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool Deleted { get; set; }
    }
}
