using MiniApi.Application.DTOs.CategoryDtos;
using MiniApi.Application.Shared;

namespace MiniApi.Application.Abstracts.Services;

public interface ICategoryService
{
    Task<BaseResponse<CategoryGetDto>> GetByIdAsync(Guid id);
    Task<BaseResponse<List<CategoryGetDto>>> GetAllAsync();
    Task<BaseResponse<CategoryGetDto>> CreateAsync(CategoryCreateDto dto);
    Task<BaseResponse<List<CategoryGetDto>>> SearchByNameAsync(string search);
    Task<BaseResponse<bool>> DeleteAsync(Guid id);
    Task<BaseResponse<CategoryGetDto>> UpdateAsync(Guid id, CategoryUpdateDto dto);
}
