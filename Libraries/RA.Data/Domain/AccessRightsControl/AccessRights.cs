using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Data.Domain.AccessRightControl
{
    public class AccessRights : BaseEntity
    {
        /// <summary>
        /// User Role Id attached to this access rights
        /// </summary>
        public Guid UserRoleId { get; set; }
        /// <summary>
        /// Role name attached to this access rights
        /// </summary>
        public string Rolename { get; set; }
        /// <summary>
        /// Record that contains List of AccessRecord in Json Format
        /// </summary>
        public string AccessRecordData { get; set; }
        public Guid CreatedById { get; set; }
        public Guid? ModifiedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool Deleted { get; set; }
    }
}
