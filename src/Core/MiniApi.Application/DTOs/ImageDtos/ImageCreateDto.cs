using Microsoft.AspNetCore.Http;

namespace MiniApi.Application.DTOs.ImageDtos;

public class ImageCreateDto
{
    public Guid ProductId { get; set; }
    public IFormFile File { get; set; }
}
