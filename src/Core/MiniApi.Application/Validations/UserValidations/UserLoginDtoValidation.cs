﻿using FluentValidation;
using MiniApi.Application.DTOs.UserDtos;

namespace MiniApi.Application.Validations.UserValidations;

public class UserLoginDtoValidation:AbstractValidator<UserLoginDto>
{
    public UserLoginDtoValidation()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email cannot be empty.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password cannot be empty.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");
    }
}
