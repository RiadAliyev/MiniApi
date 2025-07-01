namespace MiniApi.Domain.Entities;

public class User:BaseEntity
{
    public string? Name { get; set; }      
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    
    public ICollection<Product>? Products { get; set; }   
    public ICollection<Order>? Orders { get; set; }       
    public ICollection<Favourite>? Favourites { get; set; }
    public ICollection<Review>? Reviews { get; set; }

}
