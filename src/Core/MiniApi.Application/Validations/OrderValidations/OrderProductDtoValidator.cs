using FluentValidation;
using MiniApi.Application.DTOs.OrderDtos;

namespace MiniApi.Application.Validations.OrderValidations;

public class OrderProductDtoValidator : AbstractValidator<OrderProductDto>
{
    public OrderProductDtoValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ProductCount).GreaterThan(0);
    }
}
