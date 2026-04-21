using FastEndpoints;
using FluentValidation;

namespace Web.Features.Admins.Teams.Events.UpdateTeamEvent;

public class UpdateTeamEventValidator : Validator<UpdateTeamEventRequest>
{
    public UpdateTeamEventValidator()
    {
        RuleFor(x => x.Type)
            .Must(t => t == "Pratique" || t == "Match")
            .WithErrorCode("InvalidType")
            .WithMessage("Type must be 'Pratique' or 'Match'.");

        RuleFor(x => x.StartDateTime)
            .Must((req, start) => start < req.EndDateTime)
            .WithErrorCode("InvalidDateRange")
            .WithMessage("StartDateTime must be before EndDateTime.");
    }
}
