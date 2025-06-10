using Microsoft.EntityFrameworkCore;
using RA.BusinessEntities.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.BusinessEntities.App_Data
{
    public class RABusinessEntityContext : DbContext
    {
        public RABusinessEntityContext(DbContextOptions<RABusinessEntityContext> options)
            : base(options)
        {
        }

        public DbSet<BusinessEntity> BusinessEntity { get; set; }
    }
}
