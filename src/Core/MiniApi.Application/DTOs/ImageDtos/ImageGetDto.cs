namespace MiniApi.Application.DTOs.ImageDtos;

public class ImageGetDto
{
    public Guid Id { get; set; }
    public string? ImageUrl { get; set; } 
    public Guid ProductId { get; set; }
    public DateTime CreatedAt { get; set; }
}
