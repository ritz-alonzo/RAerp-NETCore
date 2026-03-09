using Microsoft.EntityFrameworkCore;
using RA.OrdersManagement.Domain.Carts;
using RA.OrdersManagement.Domain.Orders;
using RA.OrdersManagement.Domain.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.App_Data
{
    public class RAOrderManagementContext : DbContext
    {
        public RAOrderManagementContext(DbContextOptions<RAOrderManagementContext> options) 
            : base(options)
        {
        }

        public DbSet<Order> Order { get; set; }
        public DbSet<OrderItem> OrderItem { get; set; }
        public DbSet<Cart> Cart { get; set; }
        public DbSet<CartItem> CartItem { get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<PaymentItem> PaymentItem { get; set; }
    }
}
