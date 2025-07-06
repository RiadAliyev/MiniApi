using FluentValidation;
using MiniApi.Application.DTOs.ProductDtos;

namespace MiniApi.Application.Validations.ProductValidator;

public class ProductUpdateDtoValidator : AbstractValidator<ProductUpdateDto>
{
    public ProductUpdateDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
        RuleFor(x => x.Title)
            .NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price)
            .GreaterThan(0);
        RuleFor(x => x.CategoryId)
            .NotEmpty();
    }
}
