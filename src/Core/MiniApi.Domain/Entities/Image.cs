namespace MiniApi.Domain.Entities;

public class Image:BaseEntity
{
    public string? ImageUrl { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
}

