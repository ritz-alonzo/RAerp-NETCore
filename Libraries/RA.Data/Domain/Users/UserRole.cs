using RA.Core.Domain;
using System;

namespace RA.Data.Domain.Users
{
    /// <summary>
    /// Available user roles in case there 
    /// will be more user that will use this application.
    /// </summary>
    public class UserRole : BaseAdminEntity
    {
        public string Rolename { get; set; }
        public Guid? CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool Deleted { get; set; }

    }
}
