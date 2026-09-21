using FluentValidation;
using RecamNewBackend.DTOs.ListingCase;

namespace RecamNewBackend.Validators.ListingCase;

public class CreateListingCaseDtoValidator : AbstractValidator<CreateListingCaseDto>
{
    public CreateListingCaseDtoValidator()
    {
        RuleFor(x => x.Address).NotEmpty().MaximumLength(300);
        RuleFor(x => x.PropertyType).IsInEnum();
        RuleFor(x => x.SaleType).IsInEnum();
        RuleFor(x => x.Bedrooms).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Bathrooms).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Garages).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LandSize).GreaterThan(0);
    }
}
