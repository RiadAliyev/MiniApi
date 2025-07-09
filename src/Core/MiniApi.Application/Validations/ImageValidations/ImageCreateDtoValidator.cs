using FluentValidation;
using MiniApi.Application.DTOs.ImageDtos;

namespace MiniApi.Application.Validations.ImageValidations;

public class ImageCreateDtoValidator : AbstractValidator<ImageCreateDto>
{
    public ImageCreateDtoValidator()
    {
        

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId cannot be empty!");
    }
}
