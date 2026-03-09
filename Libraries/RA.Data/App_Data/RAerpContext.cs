using Microsoft.EntityFrameworkCore;
using RA.Data.Domain.AccessRightControl;
using RA.Data.Domain.Addresses;
using RA.Data.Domain.DataChanges;
using RA.Data.Domain.EntityTypes;
using RA.Data.Domain.Settings;
using RA.Data.Domain.UserActivityLogs;
using RA.Data.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Data.App_Data
{
    public class RAerpContext : DbContext
    {
        public RAerpContext(DbContextOptions<RAerpContext> options)
            : base(options)
        {
        }

        #region Admin
        public DbSet<User> User { get; set; }
        public DbSet<UserRole> UserRole { get; set; }
        public DbSet<UserUserRoleMapping> UserUserRoleMapping { get; set; }
        public DbSet<EntityType> EntityType { get; set; }
        public DbSet<Setting> Setting { get; set; }
        public DbSet<UserActivityLog> UserActivityLog { get; set; }
        public DbSet<AccessRights> AccessRights { get; set; }
        public DbSet<DataChange> DataChange { get; set; }
        public DbSet<Address> Address { get; set; }
        #endregion
    }
}
