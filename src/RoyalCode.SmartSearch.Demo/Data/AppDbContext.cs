using Microsoft.EntityFrameworkCore;
using RoyalCode.SmartSearch.Demo.Domain;

namespace RoyalCode.SmartSearch.Demo.Data;

/// <summary>
/// SQLite <see cref="DbContext"/> for the demo. Uses a single in-memory connection kept open for the
/// lifetime of the host, so the seeded database survives across requests within one run (DF8).
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Name).IsRequired();
            b.Property(c => c.Email).IsRequired();

            // Address as an owned type (same table) -> demonstrates [ComplexFilter] over an owned type.
            b.OwnsOne(c => c.MainAddress, address =>
            {
                address.Property(a => a.Street).HasColumnName("Street");
                address.Property(a => a.City).HasColumnName("City");
                address.Property(a => a.State).HasColumnName("State");
                address.Property(a => a.PostalCode).HasColumnName("PostalCode");
            });
        });

        modelBuilder.Entity<Product>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Sku).IsRequired();
            b.Property(p => p.Name).IsRequired();
            b.Property(p => p.Price).IsRequired();
            b.Property(p => p.Active).IsRequired();
        });

        modelBuilder.Entity<Order>(b =>
        {
            b.HasKey(o => o.Id);
            b.Property(o => o.Number).IsRequired();
            b.Property(o => o.CreatedAt).IsRequired();
            b.Property(o => o.Status).IsRequired();

            b.HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(b =>
        {
            b.HasKey(i => i.Id);
            b.Property(i => i.Quantity).IsRequired();
            b.Property(i => i.UnitPrice).IsRequired();

            b.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }
}
