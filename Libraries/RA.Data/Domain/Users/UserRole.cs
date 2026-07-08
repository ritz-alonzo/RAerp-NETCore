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
    }
}
