using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using MiniApi.Domain.Entities;

namespace MiniApi.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        
        builder.ToTable("Orders");
       
        builder.HasKey(x => x.Id);
       
        builder.Property(x => x.OrderDate)
            .IsRequired();
       
        builder.Property(x => x.Status)
            .HasMaxLength(100); 
       
        builder.HasMany(x => x.OrderProducts)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade); 
    }
}
