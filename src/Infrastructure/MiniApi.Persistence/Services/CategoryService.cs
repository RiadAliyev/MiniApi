using Microsoft.EntityFrameworkCore;
using MiniApi.Application.Abstracts.Repositories;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.DTOs.CategoryDtos;
using MiniApi.Application.DTOs.ProductDtos;
using MiniApi.Application.Shared;
using MiniApi.Domain.Entities;
using MiniApi.Persistence.Repositories;
using MiniApi.Persistence.Repositoriesl;
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

        // HƏLL BUDUR: Əgər ParentCategoryId var idisə, onu DB-də yoxla!
        if (dto.ParentCategoryId != null)
        {
            var parentExists = await _categoryRepository.GetAll()
                .AnyAsync(x => x.Id == dto.ParentCategoryId);
            if (!parentExists)
                return new BaseResponse<CategoryGetDto>("Parent category mövcud deyil!", HttpStatusCode.BadRequest);
        }

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

    public async Task<BaseResponse<List<CategoryGetDto>>> SearchByNameAsync(string search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return new BaseResponse<List<CategoryGetDto>>("Search text is required", null, HttpStatusCode.BadRequest);

        var loweredSearch = search.Trim().ToLower();

        var categories = await _categoryRepository
            .GetAll(true)
            .Where(x => x.Name.ToLower().Contains(loweredSearch))
            .Include(x => x.SubCategories)
            .ToListAsync();

        var dtos = categories.Select(MapToCategoryGetDto).ToList();

        if (dtos.Count == 0)
            return new BaseResponse<List<CategoryGetDto>>("No categories found", dtos, HttpStatusCode.NotFound);

        return new BaseResponse<List<CategoryGetDto>>("Success", dtos, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<bool>> DeleteAsync(Guid id)
    {
        var category = await _categoryRepository.GetByFiltered(x => x.Id == id).FirstOrDefaultAsync();
        if (category == null)
            return new BaseResponse<bool>("Category not found", false, HttpStatusCode.NotFound);

        _categoryRepository.Delete(category);
        await _categoryRepository.SaveChangeAsync();

        return new BaseResponse<bool>("Category deleted", true, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<CategoryGetDto>> UpdateAsync(Guid id, CategoryUpdateDto dto)
    {
        var category = await _categoryRepository.GetByFiltered(x => x.Id == id).FirstOrDefaultAsync();
        if (category == null)
            return new BaseResponse<CategoryGetDto>("Category not found", HttpStatusCode.NotFound);

        // Adı dəyişdirmə zamanı eyni adda kateqoriya olmamalıdır
        var exist = await _categoryRepository.GetAll()
            .AnyAsync(x => x.Name == dto.Name && x.ParentCategoryId == dto.ParentCategoryId && x.Id != id);
        if (exist)
            return new BaseResponse<CategoryGetDto>("Category with same name already exists!", HttpStatusCode.BadRequest);

        category.Name = dto.Name;
        category.ParentCategoryId = dto.ParentCategoryId;

        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangeAsync();

        var resultDto = MapToCategoryGetDto(category);
        return new BaseResponse<CategoryGetDto>("Category updated", resultDto, HttpStatusCode.OK);
    }

}
