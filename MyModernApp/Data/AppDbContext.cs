using Microsoft.EntityFrameworkCore;
using MyModernApp.Models;

namespace MyModernApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            // Create an index on the Status column for better query performance
            // This addresses the timeout issue when filtering by status='pending'
            entity.HasIndex(o => o.Status)
                .HasDatabaseName("IX_Orders_Status");

            // Create a composite index for common query patterns
            entity.HasIndex(o => new { o.Status, o.OrderDate })
                .HasDatabaseName("IX_Orders_Status_OrderDate");

            // Set precision for TotalAmount to avoid truncation
            entity.Property(o => o.TotalAmount)
                .HasPrecision(18, 2);
        });
    }
}
