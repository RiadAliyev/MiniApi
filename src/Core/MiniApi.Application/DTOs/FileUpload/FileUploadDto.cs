using Microsoft.AspNetCore.Http;

namespace MiniApi.Application.DTOs.FileUpload;

public class FileUploadDto
{
    public IFormFile File { get; set; } = null!;
}
