using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Teams.GetTeamById;

public class GetTeamByIdEndpoint : Endpoint<GetTeamByIdRequest, TeamDetailResponse>
{
    private readonly ITeamRepository _teamRepository;

    public GetTeamByIdEndpoint(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("teams/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetTeamByIdRequest req, CancellationToken ct)
    {
        var team = await _teamRepository.FindByIdWithAthletesAsync(req.Id);
        if (team is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var response = new TeamDetailResponse
        {
            Id = team.Id,
            Name = team.Name,
            CreatedAt = team.CreatedAt,
            Athletes = team.Athletes
                .Where(a => a.Active)
                .Select(a => new AthleteInTeamResponse
                {
                    Id = a.Id,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    Email = a.Email
                })
                .ToList()
        };

        await Send.OkAsync(response, cancellation: ct);
    }
}
