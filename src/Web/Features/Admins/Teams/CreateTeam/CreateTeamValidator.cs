using FastEndpoints;
using FluentValidation;

namespace Web.Features.Admins.Teams.CreateTeam;

public class CreateTeamValidator : Validator<CreateTeamRequest>
{
    public CreateTeamValidator()
    {
        RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidName")
            .WithMessage("Name should not be empty.");
    }
}
