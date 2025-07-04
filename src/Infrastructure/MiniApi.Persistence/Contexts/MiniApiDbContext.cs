using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MiniApi.Domain.Entities;

namespace MiniApi.Persistence.Contexts;

public class MiniApiDbContext:IdentityDbContext<AppUser>
{
    public MiniApiDbContext(DbContextOptions<MiniApiDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MiniApiDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Image> Images { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Favourite> Favourites { get; set; }

    
}
