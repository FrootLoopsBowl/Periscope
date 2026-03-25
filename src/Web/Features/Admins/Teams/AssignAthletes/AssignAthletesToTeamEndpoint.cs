using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Teams.AssignAthletes;

public class AssignAthletesToTeamEndpoint : Endpoint<AssignAthletesToTeamRequest>
{
    private readonly ITeamRepository _teamRepository;
    private readonly IAthleteRepository _athleteRepository;

    public AssignAthletesToTeamEndpoint(ITeamRepository teamRepository, IAthleteRepository athleteRepository)
    {
        _teamRepository = teamRepository;
        _athleteRepository = athleteRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Put("teams/{id}/athletes");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(AssignAthletesToTeamRequest req, CancellationToken ct)
    {
        var team = await _teamRepository.FindByIdAsync(req.Id);
        if (team is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // Remove athletes no longer in this team
        var currentAthletes = await _athleteRepository.GetAllAsync();
        var athletesCurrentlyInTeam = currentAthletes.Where(a => a.TeamId == req.Id).ToList();
        foreach (var athlete in athletesCurrentlyInTeam)
        {
            if (!req.AthleteIds.Contains(athlete.Id))
            {
                athlete.RemoveTeam();
                await _athleteRepository.UpdateAsync(athlete);
            }
        }

        // Assign selected athletes to this team
        foreach (var athleteId in req.AthleteIds)
        {
            var athlete = await _athleteRepository.FindByIdAsync(athleteId);
            if (athlete is null) continue;
            athlete.AssignTeam(req.Id);
            await _athleteRepository.UpdateAsync(athlete);
        }

        await Send.NoContentAsync(ct);
    }
}
