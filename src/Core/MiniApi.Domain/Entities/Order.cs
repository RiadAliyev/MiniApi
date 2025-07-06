namespace MiniApi.Domain.Entities;

public class Order:BaseEntity
{
    public DateTime OrderDate { get; set; }
    public string? Status { get; set; }

    public string BuyerId { get; set; }  // Bunu əlavə et
    public AppUser Buyer { get; set; }   // Əgər əlaqə qurmaq istəyirsənsə

    public ICollection<OrderProduct>? OrderProducts { get; set; }
}

