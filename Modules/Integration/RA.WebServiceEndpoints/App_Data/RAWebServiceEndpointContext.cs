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
    }
}
