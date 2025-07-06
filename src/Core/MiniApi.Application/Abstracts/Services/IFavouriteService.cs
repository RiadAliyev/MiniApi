using MiniApi.Application.DTOs.FavouriteDtos;
using MiniApi.Application.Shared;

namespace MiniApi.Application.Abstracts.Services;

public interface IFavouriteService
{
    Task<BaseResponse<string>> AddAsync(Guid productId, string userId);
    Task<BaseResponse<string>> RemoveAsync(Guid productId, string userId);
    Task<BaseResponse<List<FavouriteGetDto>>> GetUserFavouritesAsync(string userId);
}
