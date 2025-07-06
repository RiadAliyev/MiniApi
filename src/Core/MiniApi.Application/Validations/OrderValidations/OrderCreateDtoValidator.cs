using FluentValidation;
using MiniApi.Application.DTOs.OrderDtos;

namespace MiniApi.Application.Validations.OrderValidations;

public class OrderCreateDtoValidator : AbstractValidator<OrderCreateDto>
{
    public OrderCreateDtoValidator()
    {
        RuleFor(x => x.Products)
            .NotEmpty().WithMessage("At least one product must be ordered.");

        RuleForEach(x => x.Products)
            .SetValidator(new OrderProductDtoValidator());
    }
}
