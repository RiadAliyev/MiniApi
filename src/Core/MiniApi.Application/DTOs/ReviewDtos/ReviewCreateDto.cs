namespace MiniApi.Application.DTOs.ReviewDtos;

public class ReviewCreateDto
{
    public Guid ProductId { get; set; }
    public string Content { get; set; } = null!;
    public int Rating { get; set; }
}
