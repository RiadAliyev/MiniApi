using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.DTOs.FileUpload;
using MiniApi.Infrastructure.Services;

namespace MiniApi.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFileUpload _fileUpload;

        public FilesController(IFileUpload fileUpload)
        {
            _fileUpload = fileUpload;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] FileUploadDto dto)
        {
            var fileUrl = await _fileUpload.UploadAsync(dto.File);
            return Ok(new { FileUrl = fileUrl });
        }
    }
}
