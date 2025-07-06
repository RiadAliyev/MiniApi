using FluentValidation;
using MiniApi.Application.DTOs.CategoryDtos;

namespace MiniApi.Application.Validations.CategoryValidations;

public class CategoryCreateDtoValidator : AbstractValidator<CategoryCreateDto>
{
    public CategoryCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100);
    }
}
