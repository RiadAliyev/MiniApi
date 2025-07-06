namespace MiniApi.Domain.Entities;

public class Favourite:BaseEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public string UserId { get; set; }      // Əlavə et
    public AppUser? User { get; set; }

}

