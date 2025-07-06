namespace MiniApi.Application.DTOs.CategoryDtos;

public class CategoryGetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<CategoryGetDto> SubCategories { get; set; } = new();
}
