namespace MiniApi.Application.DTOs.OrderDtos;

public class OrderGetDto
{
    public Guid Id { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; }
    public List<OrderProductDetailDto> Products { get; set; }
}
