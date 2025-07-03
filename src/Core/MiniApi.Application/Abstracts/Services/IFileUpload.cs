using Microsoft.AspNetCore.Http;

namespace MiniApi.Application.Abstracts.Services;

public interface IFileUpload
{
    Task<string> UploadAsync(IFormFile file);

}
