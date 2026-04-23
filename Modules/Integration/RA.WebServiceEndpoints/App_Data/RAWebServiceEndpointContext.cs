using Microsoft.EntityFrameworkCore;
using RA.WebServiceEndpoints.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.WebServiceEndpoints.App_Data
{
    public class RAWebServiceEndpointContext : DbContext
    {
        public RAWebServiceEndpointContext(DbContextOptions<RAWebServiceEndpointContext> options) 
            : base(options)
        {
        }

        public DbSet<WebServiceEndpoint> WebServiceEndpoint { get; set; }

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
