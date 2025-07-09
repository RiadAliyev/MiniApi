namespace MiniApi.Application.DTOs.OrderDtos;

public class MyOrdersResponseDto
{
    public List<OrderGetDto> Orders { get; set; }
    public decimal TotalPrice { get; set; }
}
