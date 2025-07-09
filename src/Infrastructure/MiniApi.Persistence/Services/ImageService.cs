using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MiniApi.Application.Abstracts.Repositories;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.DTOs.ImageDtos;
using MiniApi.Application.Shared;
using MiniApi.Domain.Entities;
using MiniApi.Infrastructure.Services;
using MiniApi.Persistence.Repositories;

namespace MiniApi.Persistence.Services;

public class ImageService : IImageService
{
    private readonly IRepository<Image> _imageRepository;
    private readonly IProductRepository _productRepository;
    private readonly IFileUpload _fileUpload;

    public ImageService(IRepository<Image> imageRepository, IProductRepository productRepository, IFileUpload fileUpload)
    {
        _imageRepository = imageRepository;
        _productRepository = productRepository;
        _fileUpload = fileUpload;
    }

    

    // ADD IMAGE
    public async Task<BaseResponse<string>> AddImageAsync(Guid productId, IFormFile file, string userId)
    {
        // Məhsulun varlığını və sahibini yoxla
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            return new BaseResponse<string>("Product not found", HttpStatusCode.NotFound);

        if (product.OwnerId != userId)
            return new BaseResponse<string>("You can only add image to your own product", HttpStatusCode.Forbidden);

        // File upload et
        var imageUrl = await _fileUpload.UploadAsync(file);

        // Image entity-ni DB-yə əlavə et
        var image = new Image
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            ImageUrl = imageUrl,
            CreatedAt = DateTime.UtcNow
        };

        await _imageRepository.AddAsync(image);
        await _imageRepository.SaveChangeAsync();

        return new BaseResponse<string>("Image added successfully", imageUrl, HttpStatusCode.Created);
    }

    // DELETE IMAGE
    public async Task<BaseResponse<bool>> DeleteImageAsync(Guid productId, Guid imageId, string userId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            return new BaseResponse<bool>("Product not found", false, HttpStatusCode.NotFound);

        if (product.OwnerId != userId)
            return new BaseResponse<bool>("You can only delete images from your own product", false, HttpStatusCode.Forbidden);

        var image = await _imageRepository.GetByIdAsync(imageId);
        if (image == null || image.ProductId != productId)
            return new BaseResponse<bool>("Image not found or does not belong to this product", false, HttpStatusCode.NotFound);

        _imageRepository.Delete(image);
        await _imageRepository.SaveChangeAsync();

        return new BaseResponse<bool>("Image deleted successfully", true, HttpStatusCode.OK);
    }
}
