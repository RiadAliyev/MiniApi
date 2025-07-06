namespace MiniApi.Application.DTOs.OrderDtos;

public class OrderProductDetailDto
{
    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; }
    public decimal Price { get; set; }
    public int ProductCount { get; set; }
    public decimal TotalPrice { get; set; }
    public string SellerId { get; set; }
    public string SellerName { get; set; }
}
