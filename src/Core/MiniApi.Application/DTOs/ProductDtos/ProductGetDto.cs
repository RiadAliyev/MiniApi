namespace MiniApi.Application.DTOs.ProductDtos;

public class ProductGetDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; }
    public List<string> ImageUrls { get; set; }
    public string? OwnerId { get; set; }
    public string UserId { get; set; }
}
