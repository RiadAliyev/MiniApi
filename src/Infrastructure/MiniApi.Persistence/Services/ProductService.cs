using Microsoft.EntityFrameworkCore;
using MiniApi.Application.Abstracts.Repositories;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.DTOs.ProductDtos;
using MiniApi.Application.Shared;
using MiniApi.Application.Shared.Helpers;
using MiniApi.Domain.Entities;
using System.Net;

namespace MiniApi.Persistence.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<BaseResponse<ProductGetDto>> GetByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return new BaseResponse<ProductGetDto>("Product not found", HttpStatusCode.NotFound);

        var dto = MapProductToGetDto(product);
        return new BaseResponse<ProductGetDto>("Success", dto, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<List<ProductGetDto>>> GetAllAsync(Guid? categoryId, decimal? minPrice, decimal? maxPrice, string? search)
    {
        var query = _productRepository.GetAll(true);

        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId.Value);

        if (minPrice.HasValue)
            query = query.Where(x => x.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(x => x.Price <= maxPrice.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Title.Contains(search) || (x.Description != null && x.Description.Contains(search)));

        // category və images join
        query = query.Include(x => x.Category).Include(x => x.Images);

        var products = await query.ToListAsync();

        var dtos = products.Select(MapProductToGetDto).ToList();
        return new BaseResponse<List<ProductGetDto>>("Success", dtos, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<List<ProductGetDto>>> GetMyProductsAsync(string userId)
    {
        var products = await _productRepository
            .GetAll(true)
            .Where(x => x.OwnerId == userId)
            .Include(x => x.Category)
            .Include(x => x.Images)
            .ToListAsync();

        var dtos = products.Select(MapProductToGetDto).ToList();
        return new BaseResponse<List<ProductGetDto>>("Success", dtos, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<ProductGetDto>> CreateAsync(ProductCreateDto dto, string ownerId)
    {
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category == null)
            return new BaseResponse<ProductGetDto>("Category not found", HttpStatusCode.BadRequest);

        var product = new Product
        {
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            OwnerId = ownerId,
            // Images üçün əlavə mapping lazımdırsa, əlavə et.
        };

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangeAsync();

        var productDto = MapProductToGetDto(product);
        return new BaseResponse<ProductGetDto>("Product created", productDto, HttpStatusCode.Created);
    }

    public async Task<BaseResponse<ProductGetDto>> UpdateAsync(ProductUpdateDto dto, string userId)
    {
        var product = await _productRepository.GetByIdAsync(dto.Id);
        if (product == null)
            return new BaseResponse<ProductGetDto>("Product not found", HttpStatusCode.NotFound);

        if (product.OwnerId != userId)
            return new BaseResponse<ProductGetDto>("Unauthorized: You are not owner", HttpStatusCode.Forbidden);

        product.Title = dto.Title;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.CategoryId = dto.CategoryId;
        // Images üçün əlavə mapping lazımdırsa, əlavə et.

        _productRepository.Update(product);
        await _productRepository.SaveChangeAsync();

        var productDto = MapProductToGetDto(product);
        return new BaseResponse<ProductGetDto>("Product updated", productDto, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<bool>> DeleteAsync(Guid id, string userId)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return new BaseResponse<bool>("Product not found", false, HttpStatusCode.NotFound);

        if (product.OwnerId != userId)
            return new BaseResponse<bool>("Unauthorized: You are not owner", false, HttpStatusCode.Forbidden);

        _productRepository.Delete(product);
        await _productRepository.SaveChangeAsync();

        return new BaseResponse<bool>("Product deleted", true, HttpStatusCode.OK);
    }

    // Manual mapping helper
    private ProductGetDto MapProductToGetDto(Product product)
    {
        return new ProductGetDto
        {
            Id = product.Id,
            Title = product.Title,
            Description = product.Description,
            Price = product.Price,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? "",
            ImageUrls = product.Images?.Select(i => i.ImageUrl).ToList() ?? new List<string>(),
            OwnerId = product.OwnerId
        };
    }
    public async Task<BaseResponse<List<ProductGetDto>>> SearchByTitleAsync(string search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return new BaseResponse<List<ProductGetDto>>("Search text is required", null, HttpStatusCode.BadRequest);

        var loweredSearch = search.Trim().ToLower();

        var products = await _productRepository
            .GetAll(true)
            .Where(x => x.Title.ToLower().Contains(loweredSearch))
            .Include(x => x.Category)
            .Include(x => x.Images)
            .ToListAsync();

        var dtos = products.Select(MapProductToGetDto).ToList();

        if (dtos.Count == 0)
            return new BaseResponse<List<ProductGetDto>>("No products found", dtos, HttpStatusCode.NotFound);

        return new BaseResponse<List<ProductGetDto>>("Success", dtos, HttpStatusCode.OK);
    }

}
