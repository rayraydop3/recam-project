using FluentValidation;
using RecamNewBackend.DTOs.SelectedMedia;

namespace RecamNewBackend.Validators.SelectedMedia;

public class SelectMediaDtoValidator : AbstractValidator<SelectMediaDto>
{
    public SelectMediaDtoValidator()
    {
        RuleFor(x => x.ListingCaseId).GreaterThan(0);
        RuleFor(x => x.MediaAssetId).GreaterThan(0);
    }
}
