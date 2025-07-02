using System.Net;
using Microsoft.EntityFrameworkCore;
using MiniApi.Application.Abstracts.Repositories;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.DTOs.ImageDtos;
using MiniApi.Application.Shared;
using MiniApi.Domain.Entities;

namespace MiniApi.Persistence.Services;

public class ImageService : IImageService
{
    private readonly IRepository<Image> _imageRepository;

    public ImageService(IRepository<Image> imageRepository)
    {
        _imageRepository = imageRepository;
    }

    public async Task<BaseResponse<List<ImageGetDto>>> GetAllAsync()
    {
        try
        {
            var images = await _imageRepository.GetAll().ToListAsync();
            var imageDtos = images.Select(x => new ImageGetDto
            {
                Id = x.Id,
                ImageUrl = x.ImageUrl,
                ProductId = x.ProductId,
                CreatedAt = x.CreatedAt
            }).ToList();

            return new BaseResponse<List<ImageGetDto>>(
                "Bütün şəkillər uğurla qaytarıldı.",
                imageDtos,
                HttpStatusCode.OK
            );
        }
        catch (Exception ex)
        {
            return new BaseResponse<List<ImageGetDto>>(ex.Message, false, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<BaseResponse<ImageGetDto>> GetByIdAsync(Guid id)
    {
        var image = await _imageRepository.GetByIdAsync(id);
        if (image == null)
            return new BaseResponse<ImageGetDto>("Şəkil tapılmadı.", HttpStatusCode.NotFound);

        var dto = new ImageGetDto
        {
            Id = image.Id,
            ImageUrl = image.ImageUrl,
            ProductId = image.ProductId,
            CreatedAt = image.CreatedAt
        };

        return new BaseResponse<ImageGetDto>("Şəkil tapıldı.", dto, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<ImageGetDto>> GetByNameAsync(string name)
    {
        var image = await _imageRepository.GetByFiltered(x => x.ImageUrl == name).FirstOrDefaultAsync();
        if (image == null)
            return new BaseResponse<ImageGetDto>("Şəkil tapılmadı.", HttpStatusCode.NotFound);

        var dto = new ImageGetDto
        {
            Id = image.Id,
            ImageUrl = image.ImageUrl,
            ProductId = image.ProductId,
            CreatedAt = image.CreatedAt
        };

        return new BaseResponse<ImageGetDto>("Şəkil tapıldı.", dto, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<string>> AddAsync(ImageCreateDto dto)
    {
        try
        {
            var image = new Image
            {
                Id = Guid.NewGuid(),
                ImageUrl = dto.ImageUrl,
                ProductId = dto.ProductId,
                CreatedAt = DateTime.UtcNow
            };

            await _imageRepository.AddAsync(image);
            await _imageRepository.SaveChangeAsync();

            return new BaseResponse<string>("Şəkil uğurla əlavə olundu.", null, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            return new BaseResponse<string>(ex.Message, false, HttpStatusCode.InternalServerError);
        }
    }

    public async Task<BaseResponse<ImageUpdateDto>> UpdateAsync(Guid id, ImageUpdateDto dto)
    {
        var image = await _imageRepository.GetByIdAsync(id);
        if (image == null)
            return new BaseResponse<ImageUpdateDto>("Şəkil tapılmadı.", HttpStatusCode.NotFound);

        image.ImageUrl = dto.ImageUrl;
        image.ProductId = dto.ProductId;

        _imageRepository.Update(image);
        await _imageRepository.SaveChangeAsync();

        return new BaseResponse<ImageUpdateDto>("Şəkil uğurla yeniləndi.", dto, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<string>> DeleteAsync(Guid id)
    {
        var image = await _imageRepository.GetByIdAsync(id);
        if (image == null)
            return new BaseResponse<string>("Şəkil tapılmadı.", HttpStatusCode.NotFound);

        _imageRepository.Delete(image);
        await _imageRepository.SaveChangeAsync();

        return new BaseResponse<string>("Şəkil uğurla silindi.", null, HttpStatusCode.OK);
    }
}
