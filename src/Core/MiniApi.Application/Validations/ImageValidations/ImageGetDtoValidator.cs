using FluentValidation;
using MiniApi.Application.DTOs.ImageDtos;

namespace MiniApi.Application.Validations.ImageValidations;

public class ImageGetDtoValidator : AbstractValidator<ImageGetDto>
{
    public ImageGetDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.ImageUrl).NotEmpty();

        RuleFor(x => x.ProductId).NotEmpty();

        RuleFor(x => x.CreatedAt).NotEmpty();
    }
}
