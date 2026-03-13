using FastEndpoints;
using FluentValidation;

namespace Web.Features.Admins.Athletes.ResendAccessLink;

public class ResendAccessLinkValidator : Validator<ResendAccessLinkRequest>
{
    public ResendAccessLinkValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode("InvalidAthleteId")
            .WithMessage("Athlete id should not be empty.");

        RuleFor(x => x.AthletePageRelativeUrl)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidAthletePageRelativeUrl")
            .WithMessage("Athlete page relative URL should not be empty.")
            .Must(url => url.StartsWith('/'))
            .WithErrorCode("InvalidAthletePageRelativeUrl")
            .WithMessage("Athlete page relative URL format is invalid.");
    }
}
