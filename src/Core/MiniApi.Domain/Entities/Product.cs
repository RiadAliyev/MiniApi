using static System.Net.Mime.MediaTypeNames;

namespace MiniApi.Domain.Entities;

public class Product:BaseEntity
{

    public string? Name { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? OwnerId { get; set; }

    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public bool IsDeleted { get; set; }
    public ICollection<Image>? Images { get; set; }
    public ICollection<Favourite>? Favourites { get; set; }
    public ICollection<Review>? Reviews { get; set; }
    public ICollection<OrderProduct>? OrderProducts { get; set; }
}

