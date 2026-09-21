using FluentValidation;
using RecamNewBackend.DTOs.ListingCase;

namespace RecamNewBackend.Validators.ListingCase;

public class ChangeStatusDtoValidator : AbstractValidator<ChangeStatusDto>
{
    public ChangeStatusDtoValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
