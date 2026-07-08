using Microsoft.EntityFrameworkCore;
using RA.Discounts.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Discounts.App_Data
{
    public class RADiscountContext : DbContext
    {
        public RADiscountContext(DbContextOptions<RADiscountContext> options)
            : base(options)
        {
        }

        public DbSet<Discount> Discount { get; set; }
        public DbSet<DiscountRedemption> DiscountRedemption { get; set; }
    }
}
