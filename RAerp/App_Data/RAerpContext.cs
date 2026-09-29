using Microsoft.EntityFrameworkCore;
using RAerp.Domain.AccessRightControl;
using RAerp.Domain.Addresses;
using RAerp.Domain.Application;
using RAerp.Domain.DataChanges;
using RAerp.Domain.EntityAttributes;
using RAerp.Domain.EntityTypes;
using RAerp.Domain.FileManager;
using RAerp.Domain.Settings;
using RAerp.Domain.UserActivityLogs;
using RAerp.Domain.Users;
using RAerp.Security.Idempontency.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAerp.App_Data
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
        public DbSet<ApplicationSetting> ApplicationSetting { get; set; }
        public DbSet<FileEntityMapping> FileEntityMapping { get; set; }
        
        public DbSet<EntityAttribute> EntityAttribute { get; set; }
        public DbSet<EntityAttributeOption> EntityAttributeOption { get; set; }
        public DbSet<EntityAttributeValue> EntityAttributeValue { get; set; }
        // Idempotency
        public DbSet<IdempotencyRecord> IdempotencyRecord { get; set; }
        public DbSet<ExternalLogin> ExternalLogin { get; set; }
        #endregion

        #region Overrides
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return base.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                Console.WriteLine(innerMessage); // or log it
                throw;
            }
        }
        #endregion
    }
}
