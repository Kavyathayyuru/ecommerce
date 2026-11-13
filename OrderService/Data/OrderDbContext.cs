using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderOutboxMessage> OrderOutboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderOutboxMessage>()
                .HasIndex(message => message.ProcessedAtUtc);
            modelBuilder.Entity<OrderOutboxMessage>()
                .HasOne(message => message.Order)
                .WithMany()
                .HasForeignKey(message => message.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .Property(order => order.TotalPrice)
                .HasPrecision(18, 2);
        }
    }
}