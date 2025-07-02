namespace MiniApi.Application.DTOs.CategoryDtos;

public class CategoryCreateDto
{
    public string Name { get; set; } = null!;
    public Guid? ParentCategoryId { get; set; }
}
