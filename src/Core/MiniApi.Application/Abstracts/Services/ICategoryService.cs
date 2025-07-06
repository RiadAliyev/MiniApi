using MiniApi.Application.DTOs.CategoryDtos;
using MiniApi.Application.Shared;

namespace MiniApi.Application.Abstracts.Services;

public interface ICategoryService
{
    Task<BaseResponse<CategoryGetDto>> GetByIdAsync(Guid id);
    Task<BaseResponse<List<CategoryGetDto>>> GetAllAsync();
    Task<BaseResponse<CategoryGetDto>> CreateAsync(CategoryCreateDto dto);
}
