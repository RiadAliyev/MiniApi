using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using MiniApi.Domain.Entities;

namespace MiniApi.Persistence.Configurations;

public class OrderProductConfiguration : IEntityTypeConfiguration<OrderProduct>
{
    public void Configure(EntityTypeBuilder<OrderProduct> builder)
    {
        builder.ToTable("OrderProducts");

        builder.HasKey(x => new { x.OrderId, x.ProductId });
       

        builder.HasOne(x => x.Order)
            .WithMany(x => x.OrderProducts)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

       
        builder.HasOne(x => x.Product)
            .WithMany() 
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
