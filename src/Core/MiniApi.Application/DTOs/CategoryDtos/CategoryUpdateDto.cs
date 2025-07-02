namespace MiniApi.Application.DTOs.CategoryDtos;

public class CategoryUpdateDto
{
    public string Name { get; set; } = null!;
    public Guid? ParentCategoryId { get; set; }
}
