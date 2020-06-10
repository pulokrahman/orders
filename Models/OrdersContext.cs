
using Microsoft.EntityFrameworkCore;

namespace Orders.Models
{
    public class OrdersContext : DbContext
    {
        public OrdersContext(DbContextOptions<OrdersContext> options)
            : base(options)
        {
        }

        public DbSet<OrderStatus> OrderStatus { get; set; }
        public DbSet<CartItems> CartItems { get; set; }
        public DbSet<IteminOrder> IteminOrder { get; set; }
        public DbSet<IteminCart> IteminCart { get; set; }
    }
}
