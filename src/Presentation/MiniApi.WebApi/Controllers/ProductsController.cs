using System.Net;
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
using static MiniApi.Application.Shared.Permissions;

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

    
    [HttpGet("search")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Search([FromQuery] string search)
    {
        var response = await _productService.SearchByTitleAsync(search);
        return StatusCode((int)response.StatusCode, response);
    }

    
    [HttpGet("GetAll")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? categoryId, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] string? search)
    {
        var response = await _productService.GetAllAsync(categoryId, minPrice, maxPrice, search);
        return StatusCode((int)response.StatusCode, response);
    }

    
    [HttpGet("GetById")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _productService.GetByIdAsync(id);
        return StatusCode((int)response.StatusCode, response);
    }

    [Authorize(Policy = Permissions.Product.Create)]
    [HttpPost("Create")]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Create([FromForm] ProductCreateDto dto)
    {
        var userId = _userContextService.GetCurrentUserId(User); // Helperdən alınır
        var response = await _productService.CreateAsync(dto, userId);
        return StatusCode((int)response.StatusCode, response);
    }

    [Authorize(Policy = Permissions.Product.Update)]
    [HttpPut("{id}/Update")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.Forbidden)]
    public async Task<IActionResult> Update(ProductUpdateDto dto)
    {
        var userId = _userContextService.GetCurrentUserId(User);
        var userRoles = _userContextService.GetCurrentUserRoles(User);

        var response = await _productService.UpdateAsync(dto, userId, userRoles);

        return StatusCode((int)response.StatusCode, response);
    }

    [Authorize(Policy = Permissions.Product.Delete)]
    [HttpDelete("{id}/Delete")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.Forbidden)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = _userContextService.GetCurrentUserId(User);
        var userRoles = _userContextService.GetCurrentUserRoles(User);

        var response = await _productService.DeleteAsync(id, userId, userRoles);

        return StatusCode((int)response.StatusCode, response);
    }


    [Authorize(Policy = Permissions.Product.GetMy)]
    [HttpGet("GetMyProducts")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetMyProducts()
    {
        var userId = _userContextService.GetCurrentUserId(User);
        var response = await _productService.GetMyProductsAsync(userId);
        return StatusCode((int)response.StatusCode, response);
    }

    [Authorize(Policy = Permissions.Product.AddProductImage)]
    [HttpPost("add-image")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.Forbidden)]
    public async Task<IActionResult> AddImage([FromForm] ImageCreateDto dto)
    {
        var userId = _userContextService.GetCurrentUserId(User);
        // dto.ProductId, dto.File ilə işləyirsən
        var result = await _imageService.AddImageAsync(dto.ProductId, dto.File, userId);
        return StatusCode((int)result.StatusCode, result);
    }

    [Authorize(Policy = Permissions.Product.DeleteProductImage)]
    [HttpDelete("{productId}/images/{imageId}")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.Forbidden)]
    public async Task<IActionResult> DeleteImage(Guid productId, Guid imageId)
    {
        var userId = _userContextService.GetCurrentUserId(User);
        var result = await _imageService.DeleteImageAsync(productId, imageId, userId);
        return StatusCode((int)result.StatusCode, result);
    }

}
