namespace MiniApi.Application.DTOs.CategoryDtos;

public class CategoryGetDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public CategoryGetDto? ParentCategory { get; set; } 
    public List<CategoryGetDto>? SubCategories { get; set; }
}
