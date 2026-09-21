using FluentValidation;
using RecamNewBackend.DTOs.Auth;

namespace RecamNewBackend.Validators.Auth;

public class UpdatePasswordDtoValidator : AbstractValidator<UpdatePasswordDto>
{
    public UpdatePasswordDtoValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
    }
}
