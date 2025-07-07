using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniApi.Domain.Entities;

namespace MiniApi.Persistence.Configurations;


public class ImageConfiguration : IEntityTypeConfiguration<Image>
{
    public void Configure(EntityTypeBuilder<Image> builder)
    {       
        builder.ToTable("Images");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.ImageUrl)
            .HasMaxLength(500)
            .IsRequired();
        
        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Images) 
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);      
    }
}

