using FluentValidation;
using MiniApi.Application.DTOs.ReviewDtos;

namespace MiniApi.Application.Validations.ReviewValidations;

public class ReviewCreateDtoValidator : AbstractValidator<ReviewCreateDto>
{
    public ReviewCreateDtoValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId boş ola bilməz");
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Məzmun boş ola bilməz")
            .MaximumLength(1000).WithMessage("Məzmun ən çox 1000 simvol ola bilər");
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Qiymət 1 ilə 5 arasında olmalıdır");
    }
}
