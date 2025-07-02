namespace MiniApi.Application.DTOs.ImageDtos;

public class ImageCreateDto
{
    public string? ImageUrl { get; set; } 
    public Guid ProductId { get; set; }
}
