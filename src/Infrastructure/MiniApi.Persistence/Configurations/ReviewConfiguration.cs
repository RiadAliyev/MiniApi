using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using MiniApi.Domain.Entities;

namespace MiniApi.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
       
        builder.ToTable("Reviews");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Reviews) 
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
       
        builder.Property(x => x.Content)
            .HasMaxLength(1000); 

        
        builder.Property(x => x.Rating)
            .IsRequired();
    }
}
