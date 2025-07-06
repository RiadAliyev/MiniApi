using FluentValidation;
using MiniApi.Application.DTOs.FavouriteDtos;

namespace MiniApi.Application.Validations.FavouriteValidations;

public class FavouriteCreateDtoValidator : AbstractValidator<FavouriteCreateDto>
{
    public FavouriteCreateDtoValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId boş ola bilməz.");
    }
}
