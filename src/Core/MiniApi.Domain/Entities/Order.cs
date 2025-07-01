namespace MiniApi.Domain.Entities;

public class Order:BaseEntity
{
    public DateTime OrderDate { get; set; }
    public string? Status { get; set; }

    public ICollection<OrderProduct>? OrderProducts { get; set; }
}

