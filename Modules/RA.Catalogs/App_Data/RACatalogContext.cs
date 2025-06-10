using Microsoft.EntityFrameworkCore;
using RA.Catalogs.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Catalogs.App_Data
{
    public class RACatalogContext : DbContext
    {
        public RACatalogContext(DbContextOptions<RACatalogContext> options)
            : base(options)
        {
        }

        public DbSet<Catalog> Catalog { get; set; }
    }
}
