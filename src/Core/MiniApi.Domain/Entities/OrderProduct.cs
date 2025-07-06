namespace MiniApi.Domain.Entities;

public class OrderProduct
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public int ProductCount { get; set; }
    public decimal TotalPrice { get; set; }
    public string SellerId { get; set; }  // (Product.OwnerId-dən avtomatik gəlir)
}

