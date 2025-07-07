using MiniApi.Application.DTOs.ReviewDtos;
using MiniApi.Application.Shared;

namespace MiniApi.Application.Abstracts.Services;

public interface IReviewService
{
    Task<BaseResponse<string>> CreateAsync(ReviewCreateDto dto, string userId);
    Task<BaseResponse<string>> DeleteAsync(Guid reviewId, string userId);
}
