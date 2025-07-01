using Microsoft.EntityFrameworkCore;
using MiniApi.Domain.Entities;

namespace MiniApi.Persistence.Contexts;

public class MiniApiDbContext:DbContext
{
    public MiniApiDbContext(DbContextOptions<MiniApiDbContext> options):base(options)
    { 
    }


    public DbSet<Image> Images { get; set; }
}
