using FluentValidation;
using RecamNewBackend.DTOs.Agent;

namespace RecamNewBackend.Validators.Agent;

public class AddAgentDtoValidator : AbstractValidator<AddAgentDto>
{
    public AddAgentDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}
