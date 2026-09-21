using FluentValidation;
using RecamNewBackend.DTOs.Auth;

namespace RecamNewBackend.Validators.Auth;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
