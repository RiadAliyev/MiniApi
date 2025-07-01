using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using MiniApi.Domain.Entities;

namespace MiniApi.Persistence.Configurations;

public class FavouriteConfiguration : IEntityTypeConfiguration<Favourite>
{
    public void Configure(EntityTypeBuilder<Favourite> builder)
    {
        
        builder.ToTable("Favourites");

        
        builder.HasKey(x => x.Id);

        
        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.HasOne(x => x.Product)
            .WithMany() // əgər Product-da ICollection<Favourite> yoxdursa, WithMany() boş qalır
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade); // Product silinərsə, Favourite-lər də silinsin
        
    }
}
