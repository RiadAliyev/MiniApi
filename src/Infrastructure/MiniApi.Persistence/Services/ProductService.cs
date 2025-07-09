using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MiniApi.Application.Abstracts.Repositories;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.DTOs.ProductDtos;
using MiniApi.Application.Shared;
using MiniApi.Application.Shared.Helpers;
using MiniApi.Domain.Entities;
using MiniApi.Domain.Enums;
using MiniApi.Infrastructure.Services;
using MiniApi.Persistence.Repositories;
using System.Net;

namespace MiniApi.Persistence.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IImageRepository _imageRepository;
    private readonly IFileUpload _fileUpload;

    public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository, IFileUpload fileUpload, IImageRepository imageRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _fileUpload = fileUpload;
        _imageRepository = imageRepository;
    }

    public async Task<BaseResponse<ProductGetDto>> GetByIdAsync(Guid id)
    {
        var product = await _productRepository.GetAll(true)
            .Where(x => x.Id == id && !x.IsDeleted)
            .Include(x => x.Category)
            .Include(x => x.Images)
            .FirstOrDefaultAsync();

        if (product == null)
            return new BaseResponse<ProductGetDto>("Product not found", HttpStatusCode.NotFound);

        var dto = MapProductToGetDto(product);
        return new BaseResponse<ProductGetDto>("Success", dto, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<List<ProductGetDto>>> GetAllAsync(Guid? categoryId, decimal? minPrice, decimal? maxPrice, string? search)
    {
        var query = _productRepository.GetAll(true)
            .Where(x => !x.IsDeleted);

        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId.Value);

        if (minPrice.HasValue)
            query = query.Where(x => x.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(x => x.Price <= maxPrice.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Title.Contains(search) || (x.Description != null && x.Description.Contains(search)));

        query = query.Include(x => x.Category).Include(x => x.Images);

        var products = await query.ToListAsync();
        var dtos = products.Select(MapProductToGetDto).ToList();
        return new BaseResponse<List<ProductGetDto>>("Success", dtos, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<List<ProductGetDto>>> GetMyProductsAsync(string userId)
    {
        var products = await _productRepository
            .GetAll(true)
            .Where(x => x.OwnerId == userId && !x.IsDeleted)
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
            Images = new List<Image>()
        };

        if (dto.Images != null && dto.Images.Any())
        {
            foreach (var formFile in dto.Images)
            {
                var imageUrl = await _fileUpload.UploadAsync(formFile);
                product.Images.Add(new Image
                {
                    Id = Guid.NewGuid(),
                    ImageUrl = imageUrl,
                    ProductId = product.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangeAsync();

        var productDto = MapProductToGetDto(product);
        return new BaseResponse<ProductGetDto>("Product created", productDto, HttpStatusCode.Created);
    }


    public async Task<BaseResponse<ProductGetDto>> UpdateAsync(ProductUpdateDto dto, string userId, List<string> userRoles)
    {
        var product = await _productRepository.GetByIdAsync(dto.Id);
        if (product == null)
            return new BaseResponse<ProductGetDto>("Product not found", HttpStatusCode.NotFound);

        // Əgər admin və ya owner-dirsə, icazə ver
        if (product.OwnerId != userId && !(userRoles.Contains("Admin")))
            return new BaseResponse<ProductGetDto>("Unauthorized: You are not owner", HttpStatusCode.Forbidden);

        product.Title = dto.Title;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.CategoryId = dto.CategoryId;
        // ... Qalan mappinglər

        _productRepository.Update(product);
        await _productRepository.SaveChangeAsync();

        var productDto = MapProductToGetDto(product);
        return new BaseResponse<ProductGetDto>("Product updated", productDto, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<bool>> DeleteAsync(Guid id, string userId, List<string> userRoles)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return new BaseResponse<bool>("Product not found", false, HttpStatusCode.NotFound);

        if (product.OwnerId != userId && !(userRoles.Contains("Admin")))
            return new BaseResponse<bool>("Unauthorized: You are not owner", false, HttpStatusCode.Forbidden);

        product.IsDeleted = true; // <--- Soft delete
        _productRepository.Update(product); // <--- Update çağır
        await _productRepository.SaveChangeAsync();

        return new BaseResponse<bool>("Product deleted (soft)", true, HttpStatusCode.OK);
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
            .Where(x => !x.IsDeleted && x.Title.ToLower().Contains(loweredSearch))
            .Include(x => x.Category)
            .Include(x => x.Images)
            .ToListAsync();

        var dtos = products.Select(MapProductToGetDto).ToList();

        if (dtos.Count == 0)
            return new BaseResponse<List<ProductGetDto>>("No products found", dtos, HttpStatusCode.NotFound);

        return new BaseResponse<List<ProductGetDto>>("Success", dtos, HttpStatusCode.OK);
    }
    

}
