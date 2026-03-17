using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Teams.DeleteTeam;

public class DeleteTeamEndpoint : Endpoint<DeleteTeamRequest, EmptyResponse>
{
    private readonly ITeamRepository _teamRepository;
    private readonly IAthleteRepository _athleteRepository;

    public DeleteTeamEndpoint(ITeamRepository teamRepository, IAthleteRepository athleteRepository)
    {
        _teamRepository = teamRepository;
        _athleteRepository = athleteRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Delete("teams/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeleteTeamRequest req, CancellationToken ct)
    {
        var team = await _teamRepository.FindByIdWithAthletesAsync(req.Id);
        if (team is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        foreach (var athlete in team.Athletes.ToList())
        {
            athlete.RemoveTeam();
            await _athleteRepository.UpdateAsync(athlete);
        }

        await _teamRepository.DeleteAsync(team);
        await Send.NoContentAsync(ct);
    }
}
