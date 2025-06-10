using Microsoft.EntityFrameworkCore;
using RA.Categories.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Categories.App_Data
{
    public class RACategoryContext : DbContext
    {
        public RACategoryContext(DbContextOptions<RACategoryContext> options) 
            : base(options)
        {
        }

        public DbSet<Category> Category { get; set; }
        public DbSet<CategoryEntityMapping> CategoryEntityMapping { get; set; }
    }
}
