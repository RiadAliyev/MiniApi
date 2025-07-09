using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniApi.Application.DTOs.ProductDtos;
using MiniApi.Application.Shared;

namespace MiniApi.Application.Abstracts.Services;

public interface IProductService
{
    Task<BaseResponse<ProductGetDto>> GetByIdAsync(Guid id);
    Task<BaseResponse<List<ProductGetDto>>> GetAllAsync(Guid? categoryId, decimal? minPrice, decimal? maxPrice, string? search);
    Task<BaseResponse<List<ProductGetDto>>> GetMyProductsAsync(string userId);
    Task<BaseResponse<ProductGetDto>> CreateAsync(ProductCreateDto dto, string ownerId);
    Task<BaseResponse<ProductGetDto>> UpdateAsync(ProductUpdateDto dto, string userId, List<string> userRoles);
    Task<BaseResponse<bool>> DeleteAsync(Guid id, string userId, List<string> userRoles);
    Task<BaseResponse<List<ProductGetDto>>> SearchByTitleAsync(string search);
}
