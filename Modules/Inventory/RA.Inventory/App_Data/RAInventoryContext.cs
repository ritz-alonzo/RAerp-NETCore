using Microsoft.EntityFrameworkCore;
using RA.Inventory.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Inventory.App_Data
{
    public class RAInventoryContext : DbContext
    {
        public RAInventoryContext(DbContextOptions<RAInventoryContext> options)
            : base(options)
        {
        }

        public DbSet<InventoryStock> InventoryStock { get; set; }
        public DbSet<InventoryTransaction> InventoryTransaction { get; set; }
        public DbSet<InventoryReservation> InventoryReservation { get; set; }

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
