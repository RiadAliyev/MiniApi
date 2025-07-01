namespace MiniApi.Domain.Entities;

public class Order:BaseEntity
{
    public Guid BuyerId { get; set; }
    public User? Buyer { get; set; }

    public DateTime OrderDate { get; set; }
    public string? Status { get; set; }

    public ICollection<OrderProduct>? OrderProducts { get; set; }
}

