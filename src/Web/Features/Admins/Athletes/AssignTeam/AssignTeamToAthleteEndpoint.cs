using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Athletes.AssignTeam;

public class AssignTeamToAthleteEndpoint : Endpoint<AssignTeamToAthleteRequest>
{
    private readonly IAthleteRepository _athleteRepository;
    private readonly ITeamRepository _teamRepository;

    public AssignTeamToAthleteEndpoint(IAthleteRepository athleteRepository, ITeamRepository teamRepository)
    {
        _athleteRepository = athleteRepository;
        _teamRepository = teamRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Put("athletes/{id}/team");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(AssignTeamToAthleteRequest req, CancellationToken ct)
    {
        var athlete = await _athleteRepository.FindByIdAsync(req.Id);
        if (athlete is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (req.TeamId.HasValue)
        {
            var team = await _teamRepository.FindByIdAsync(req.TeamId.Value);
            if (team is null)
            {
                await Send.NotFoundAsync(ct);
                return;
            }
            athlete.AssignTeam(req.TeamId.Value);
        }
        else
        {
            athlete.RemoveTeam();
        }

        await _athleteRepository.UpdateAsync(athlete);
        await Send.NoContentAsync(ct);
    }
}
