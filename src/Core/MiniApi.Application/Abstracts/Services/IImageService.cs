using Microsoft.AspNetCore.Http;
using MiniApi.Application.DTOs.ImageDtos;
using MiniApi.Application.Shared;

namespace MiniApi.Application.Abstracts.Services;

public interface IImageService
{

    
    
    Task<BaseResponse<string>> AddImageAsync(Guid productId, IFormFile file, string userId);
    Task<BaseResponse<bool>> DeleteImageAsync(Guid productId, Guid imageId, string userId);
}

