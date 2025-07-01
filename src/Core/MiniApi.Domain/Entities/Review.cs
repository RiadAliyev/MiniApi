namespace MiniApi.Domain.Entities;

public class Review:BaseEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }


    public string? Content { get; set; }  //Mezmun
    public int Rating { get; set; }
}

