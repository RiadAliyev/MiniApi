using Microsoft.EntityFrameworkCore;
using MiniApi.Application.Abstracts.Repositories;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.DTOs.CategoryDtos;
using MiniApi.Application.Shared;
using MiniApi.Domain.Entities;
using System.Linq.Expressions;
using System.Net;

namespace MiniApi.Persistence.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<BaseResponse<CategoryGetDto>> GetByIdAsync(Guid id)
    {
        var category = await _categoryRepository.GetByFiltered(x => x.Id == id, new[] { (Expression<Func<Category, object>>)(c => c.SubCategories) }).FirstOrDefaultAsync();
        if (category == null)
            return new BaseResponse<CategoryGetDto>("Category not found", HttpStatusCode.NotFound);

        var dto = MapToCategoryGetDto(category);
        return new BaseResponse<CategoryGetDto>("Success", dto, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<List<CategoryGetDto>>> GetAllAsync()
    {
        // Root (main) category-ləri və nested sub-category-ləri gətir
        var categories = await _categoryRepository
            .GetAll(true)
            .Where(x => x.ParentCategoryId == null)
            .Include(x => x.SubCategories)
            .ToListAsync();

        var dtos = categories.Select(MapToCategoryGetDto).ToList();
        return new BaseResponse<List<CategoryGetDto>>("Success", dtos, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<CategoryGetDto>> CreateAsync(CategoryCreateDto dto)
    {
        // Eyni adda category varsa error ver
        var exist = _categoryRepository.GetAll().Any(x => x.Name == dto.Name && x.ParentCategoryId == dto.ParentCategoryId);
        if (exist)
            return new BaseResponse<CategoryGetDto>("Category with same name already exists!", HttpStatusCode.BadRequest);

        var category = new Category
        {
            Name = dto.Name,
            ParentCategoryId = dto.ParentCategoryId
        };

        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangeAsync();

        var resultDto = MapToCategoryGetDto(category);
        return new BaseResponse<CategoryGetDto>("Category created", resultDto, HttpStatusCode.Created);
    }

    // Nested (iç-içə) mappinq helper
    private CategoryGetDto MapToCategoryGetDto(Category category)
    {
        return new CategoryGetDto
        {
            Id = category.Id,
            Name = category.Name,
            SubCategories = category.SubCategories?.Select(MapToCategoryGetDto).ToList() ?? new List<CategoryGetDto>()
        };
    }
}
