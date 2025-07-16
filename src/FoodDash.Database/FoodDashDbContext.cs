using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FoodDash.Database.Entities;

namespace FoodDash.Database;

public class FoodDashDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public FoodDashDbContext(DbContextOptions<FoodDashDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Restaurant> Restaurants { get; set; }
    public DbSet<MenuCategory> MenuCategories { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<DeliveryPartner> DeliveryPartners { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Coupon> Coupons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User relationships
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasOne(u => u.Customer)
                .WithOne(c => c.User)
                .HasForeignKey<Customer>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(u => u.DeliveryPartner)
                .WithOne(dp => dp.User)
                .HasForeignKey<DeliveryPartner>(dp => dp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.OwnedRestaurants)
                .WithOne(r => r.Owner)
                .HasForeignKey(r => r.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(u => u.CustomerOrders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(u => u.DeliveredOrders)
                .WithOne(o => o.DeliveryPartner)
                .HasForeignKey(o => o.DeliveryPartnerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Restaurant relationships
        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.HasMany(r => r.MenuCategories)
                .WithOne(mc => mc.Restaurant)
                .HasForeignKey(mc => mc.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(r => r.MenuItems)
                .WithOne(mi => mi.Restaurant)
                .HasForeignKey(mi => mi.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(r => r.Orders)
                .WithOne(o => o.Restaurant)
                .HasForeignKey(o => o.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(r => r.Reviews)
                .WithOne(rev => rev.Restaurant)
                .HasForeignKey(rev => rev.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure MenuCategory relationships
        modelBuilder.Entity<MenuCategory>(entity =>
        {
            entity.HasMany(mc => mc.MenuItems)
                .WithOne(mi => mi.Category)
                .HasForeignKey(mi => mi.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure MenuItem relationships
        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasMany(mi => mi.OrderItems)
                .WithOne(oi => oi.MenuItem)
                .HasForeignKey(oi => oi.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(mi => mi.CartItems)
                .WithOne(ci => ci.MenuItem)
                .HasForeignKey(ci => ci.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Order relationships
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(o => o.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(o => o.OrderNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(o => o.OrderNumber)
                .IsUnique();
        });

        // Configure Customer relationships
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasMany(c => c.CartItems)
                .WithOne(ci => ci.Customer)
                .HasForeignKey(ci => ci.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Review relationships
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasOne(r => r.MenuItem)
                .WithMany()
                .HasForeignKey(r => r.MenuItemId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure decimal precision for all decimal properties
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            if (property.GetColumnType() == null)
            {
                property.SetColumnType("decimal(18,2)");
            }
        }

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Identity Roles
        modelBuilder.Entity<IdentityRole<int>>().HasData(
            new IdentityRole<int> { Id = 1, Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole<int> { Id = 2, Name = "Customer", NormalizedName = "CUSTOMER" },
            new IdentityRole<int> { Id = 3, Name = "RestaurantOwner", NormalizedName = "RESTAURANTOWNER" },
            new IdentityRole<int> { Id = 4, Name = "DeliveryPartner", NormalizedName = "DELIVERYPARTNER" }
        );
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is ITimestampEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (ITimestampEntity)entry.Entity;
            entity.UpdatedAt = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
        }
    }
}

// Interface for entities with timestamps
public interface ITimestampEntity
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}