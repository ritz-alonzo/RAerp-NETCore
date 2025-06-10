using System;

namespace RA.Data.Domain.Users
{
    public class UserUserRoleMapping
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid UserRoleId { get; set; }
    }
}
