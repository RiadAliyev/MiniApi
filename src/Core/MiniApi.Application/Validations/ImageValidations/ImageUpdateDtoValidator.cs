using FluentValidation;
using MiniApi.Application.DTOs.ImageDtos;

namespace MiniApi.Application.Validations.ImageValidations;

public class ImageUpdateDtoValidator : AbstractValidator<ImageUpdateDto>
{
    public ImageUpdateDtoValidator()
    {
        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("ImageUrl cannot be empty!")
            .MaximumLength(255).WithMessage("ImageUrl is too long! Maximum 255 characters allowed.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId cannot be empty!");
    }
}
