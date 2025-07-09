using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MiniApi.Application.Abstracts.Services;


namespace MiniApi.Infrastructure.Services;

public class FileUpload : IFileUpload
{

    private readonly IWebHostEnvironment _env;

    public FileUpload(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> UploadAsync(IFormFile file)
    {
        if (string.IsNullOrEmpty(_env.WebRootPath))
            throw new InvalidOperationException("WebRootPath is null. Ensure that wwwroot folder exists and application is configured properly.");
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/{fileName}";
    }
}
