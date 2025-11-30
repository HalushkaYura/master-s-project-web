using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SmartClass.Application.Contracts.Auth;

namespace SmartClass.Application.Features.Auth.UserValidators
{
    public class UserRegistrationValidation : AbstractValidator<RegisterDto>
    {
        public UserRegistrationValidation()
        {
            RuleFor(user => user.FirstName)
                .NotNull()
                .Length(3, 50);
            RuleFor(user => user.LastName)
                .NotNull()
                .Length(3, 50);

            RuleFor(user => user.Email)
                .NotNull()
                .EmailAddress();

            RuleFor(user => user.Password)
                .NotEmpty()
                .MinimumLength(6)
                .Matches("[A-Z]").WithMessage("{PropertyName} must contain one or more capital letters.")
                .Matches("[a-z]").WithMessage("{PropertyName} must contain one or more lowercase letters.")
                .Matches(@"\d").WithMessage("{PropertyName} must contain one or more digits.")
                .Matches(@"[][""!@$%^&*(){}:;<>,.?/+_=|'~\\-]").WithMessage("{PropertyName} must contain one or more special characters.")
                .Matches("^[^£# “”]*$").WithMessage("{PropertyName} must not contain the following characters £ # “” or spaces.");

        }

    }
}
