namespace MiniApi.Domain.Entities;

public class Favourite:BaseEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

}

