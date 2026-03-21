using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Athletes.ToggleAthleteInjured;

public class ToggleAthleteInjuredEndpoint : Endpoint<ToggleAthleteInjuredRequest>
{
    private readonly IAthleteRepository _athleteRepository;

    public ToggleAthleteInjuredEndpoint(IAthleteRepository athleteRepository)
    {
        _athleteRepository = athleteRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Patch("athletes/{id}/injured");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(ToggleAthleteInjuredRequest req, CancellationToken ct)
    {
        var athlete = await _athleteRepository.FindByIdAsync(req.Id);
        if (athlete == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        athlete.SetIsInjured(req.IsInjured);
        await _athleteRepository.UpdateAsync(athlete);

        await Send.NoContentAsync(ct);
    }
}
