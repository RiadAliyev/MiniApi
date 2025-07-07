using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.DTOs.FileUpload;
using MiniApi.Application.DTOs.ImageDtos;
using MiniApi.Application.DTOs.ProductDtos;
using MiniApi.Application.DTOs.ReviewDtos;
using MiniApi.Application.Shared;
using MiniApi.Application.Shared.Helpers;
using MiniApi.Infrastructure.Services;
using MiniApi.Persistence.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MiniApi.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IUserContextService _userContextService;
    private readonly IImageService _imageService;

    private readonly IFileUpload _fileUpload;
    

    public ProductsController(IProductService productService, IUserContextService userContextService, IImageService imageService, IFileUpload fileUpload)
    {
        _productService = productService;
        _userContextService = userContextService;
        _imageService = imageService;
        _fileUpload = fileUpload;
    }

    [Authorize]
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string search)
    {
        var response = await _productService.SearchByTitleAsync(search);
        return StatusCode((int)response.StatusCode, response);
    }

    
    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll([FromQuery] Guid? categoryId, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] string? search)
    {
        var response = await _productService.GetAllAsync(categoryId, minPrice, maxPrice, search);
        return StatusCode((int)response.StatusCode, response);
    }

    
    [HttpGet("GetById")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _productService.GetByIdAsync(id);
        return StatusCode((int)response.StatusCode, response);
    }

    [Authorize(Policy = Permissions.Product.Create)]
    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
    {
        var userId = _userContextService.GetCurrentUserId(User); // Helperdən alınır
        var response = await _productService.CreateAsync(dto, userId);
        return StatusCode((int)response.StatusCode, response);
    }

    [Authorize(Policy = Permissions.Product.Update)]
    [HttpPut("{id}/Update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductUpdateDto dto)
    {
        if (id != dto.Id)
            return BadRequest("Id mismatch");

        var userId = _userContextService.GetCurrentUserId(User);
        var response = await _productService.UpdateAsync(dto, userId);
        return StatusCode((int)response.StatusCode, response);
    }

    [Authorize(Policy = Permissions.Product.Delete)]
    [HttpDelete("{id}/Delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = _userContextService.GetCurrentUserId(User);
        var response = await _productService.DeleteAsync(id, userId);
        return StatusCode((int)response.StatusCode, response);
    }


    [Authorize(Policy = Permissions.Product.GetMy)]
    [HttpGet("GetMyProducts")]
    public async Task<IActionResult> GetMyProducts()
    {
        var userId = _userContextService.GetCurrentUserId(User);
        var response = await _productService.GetMyProductsAsync(userId);
        return StatusCode((int)response.StatusCode, response);
    }

    [Authorize(Policy = Permissions.Product.AddProductImage)]
    [HttpPost("{productId}/Add-Images")]
    public async Task<IActionResult> AddImage(Guid productId, [FromBody] ImageCreateDto dto)
    {
        // ProductId-ni DTO-ya təyin et (optional: dto-dan da ala bilərsən, amma URL daha düzgündür)
        dto.ProductId = productId;

        var userId = _userContextService.GetCurrentUserId(User);

        // Sellerin öz məhsuluna əlavə etdiyi yoxlanılsın (optional: əgər servisdə yoxlayırsansa, burda lazım deyil)
        var product = await _productService.GetByIdAsync(productId);
        if (product == null || product.Data == null)
            return NotFound("Product tapılmadı.");

        

        var response = await _imageService.AddAsync(dto);
        return StatusCode((int)response.StatusCode, response);
    }

    [Authorize(Policy = Permissions.Product.DeleteProductImage)]
    [HttpDelete("{productId}/images/{imageId}/Delete-Image")]
    public async Task<IActionResult> DeleteImage(Guid productId, Guid imageId)
    {
        var userId = _userContextService.GetCurrentUserId(User);

        // Əlavə security: şəkil həqiqətən bu məhsul üçündür və bu userin məhsuludurmu?
        var product = await _productService.GetByIdAsync(productId);
        if (product == null || product.Data == null)
            return NotFound("Product tapılmadı.");

        if (product.Data.UserId != userId)
            return Forbid("Yalnız öz məhsulunuzun şəkilini silə bilərsiniz.");

        // Əgər şəkil məhsula aid deyil, 404 ver
        var image = await _imageService.GetByIdAsync(imageId);
        if (image == null || image.Data == null || image.Data.ProductId != productId)
            return NotFound("Şəkil tapılmadı və ya məhsula aid deyil.");

        var response = await _imageService.DeleteAsync(imageId);
        return StatusCode((int)response.StatusCode, response);
    }


    [Authorize(Policy = Permissions.Product.Create)]
    [HttpPost("File-Upload")]
    public async Task<IActionResult> Upload([FromForm] FileUploadDto dto)
    {
        var fileUrl = await _fileUpload.UploadAsync(dto.File);
        return Ok(new { FileUrl = fileUrl });
    }

    
}
