using Microsoft.AspNetCore.Http;

namespace MiniApi.Application.DTOs.ProductDtos;

public class ProductCreateDto
{
  
    public string Title { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public Guid CategoryId { get; set; }
    public List<IFormFile>? Images { get; set; }

}
