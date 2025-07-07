namespace MiniApi.Domain.Entities;

public class Review:BaseEntity
{
    public string UserId { get; set; }

    public AppUser User { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }


    public string? Content { get; set; }  //Mezmun
    public int Rating { get; set; }
}

