using Microsoft.EntityFrameworkCore;
using MiniApi.Application.Abstracts.Repositories;
using MiniApi.Application.Abstracts.Services;
using MiniApi.Application.DTOs.CategoryDtos;
using MiniApi.Application.DTOs.ProductDtos;
using MiniApi.Application.Shared;
using MiniApi.Domain.Entities;
using MiniApi.Infrastructure.Services;
using MiniApi.Persistence.Repositories;
using MiniApi.Persistence.Repositoriesl;
using System.Linq.Expressions;
using System.Net;

namespace MiniApi.Persistence.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IRedisService _redisService;

    public CategoryService(ICategoryRepository categoryRepository,IRedisService redisService)
    {
        _categoryRepository = categoryRepository;
        _redisService = redisService;
    }

    public async Task<BaseResponse<CategoryGetDto>> GetByIdAsync(Guid id)
    {
        var category = await _categoryRepository.GetByFiltered(
            x => x.Id == id && !x.IsDeleted, // <--- filter əlavə olundu
            new[] { (Expression<Func<Category, object>>)(c => c.SubCategories) }
        ).FirstOrDefaultAsync();

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
            .Where(x => x.ParentCategoryId == null && !x.IsDeleted) // <--- filter əlavə olundu
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
        .AnyAsync(x => x.Id == dto.ParentCategoryId && !x.IsDeleted); // <--- filter əlavə olundu
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
        var cacheKey = $"search:categories:{loweredSearch}";

        var cached = await _redisService.GetAsync<List<CategoryGetDto>>(cacheKey);
        if (cached is not null && cached.Any())
            return new BaseResponse<List<CategoryGetDto>>("From Redis Cache", cached, HttpStatusCode.OK);

        var categories = await _categoryRepository
            .GetAll(true)
            .Where(x => !x.IsDeleted && x.Name.ToLower().Contains(loweredSearch))
            .Include(x => x.SubCategories)
            .ToListAsync();

        var dtos = categories.Select(MapToCategoryGetDto).ToList();

        if (dtos.Count == 0)
            return new BaseResponse<List<CategoryGetDto>>("No categories found", dtos, HttpStatusCode.NotFound);

        await _redisService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(30));

        return new BaseResponse<List<CategoryGetDto>>("From DB", dtos, HttpStatusCode.OK);
    }


    public async Task<BaseResponse<bool>> DeleteAsync(Guid id)
    {
        var category = await _categoryRepository.GetByFiltered(x => x.Id == id).FirstOrDefaultAsync();
        if (category == null)
            return new BaseResponse<bool>("Category not found", false, HttpStatusCode.NotFound);

        category.IsDeleted = true; // <--- Soft delete
        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangeAsync();

        return new BaseResponse<bool>("Category deleted (soft)", true, HttpStatusCode.OK);
    }

    public async Task<BaseResponse<CategoryGetDto>> UpdateAsync(Guid id, CategoryUpdateDto dto)
    {
        var category = await _categoryRepository.GetByFiltered(x => x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();
        if (category == null)
            return new BaseResponse<CategoryGetDto>("Category not found", HttpStatusCode.NotFound);

        // Eyni adda category varsa error ver (yalnız aktiv olanlar)
        var exist = await _categoryRepository.GetAll()
            .AnyAsync(x => x.Name == dto.Name && x.ParentCategoryId == dto.ParentCategoryId && x.Id != id && !x.IsDeleted);
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
