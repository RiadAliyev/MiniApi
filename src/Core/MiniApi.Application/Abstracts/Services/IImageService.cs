using MiniApi.Application.DTOs.ImageDtos;
using MiniApi.Application.Shared;

namespace MiniApi.Application.Abstracts.Services;

public interface IImageService
{

    Task<BaseResponse<List<ImageGetDto>>> GetAllAsync();
    Task<BaseResponse<ImageGetDto>> GetByIdAsync(Guid id);
    Task<BaseResponse<ImageGetDto>> GetByNameAsync(string name);
    Task<BaseResponse<string>> AddAsync(ImageCreateDto dto); 
    Task<BaseResponse<ImageUpdateDto>> UpdateAsync(Guid id, ImageUpdateDto dto);
    Task<BaseResponse<string>> DeleteAsync(Guid id);
}

