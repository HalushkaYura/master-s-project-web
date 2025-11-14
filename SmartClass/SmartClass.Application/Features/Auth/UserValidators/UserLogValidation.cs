using FluentValidation;
using SmartClass.Application.Contracts.Auth;

namespace SmartClass.Application.Features.Auth.UserValidators
{
    public class UserLogValidation : AbstractValidator<LoginDto>
{
    public UserLogValidation()
    {
        RuleFor(user => user.Email)
            .NotEmpty()
            .NotNull();

        RuleFor(user => user.Password)
            .NotEmpty()
            .NotNull();
    }
}
}
