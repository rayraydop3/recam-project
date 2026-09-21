using FluentValidation;
using RecamNewBackend.DTOs.CaseContact;

namespace RecamNewBackend.Validators.CaseContact;

public class AddCaseContactDtoValidator : AbstractValidator<AddCaseContactDto>
{
    public AddCaseContactDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty();
    }
}
