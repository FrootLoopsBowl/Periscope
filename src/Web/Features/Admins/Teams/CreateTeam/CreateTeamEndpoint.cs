using Domain.Entities;
using Domain.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Features.Common;

namespace Web.Features.Admins.Teams.CreateTeam;

public class CreateTeamEndpoint : EndpointWithSanitizedRequest<CreateTeamRequest, TeamResponse>
{
    private readonly ITeamRepository _teamRepository;

    public CreateTeamEndpoint(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("teams");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateTeamRequest req, CancellationToken ct)
    {
        if (await _teamRepository.ExistsByNameAsync(req.Name))
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                errors = new[] { new { errorType = "TeamWithNameAlreadyExists", errorMessage = "A team with this name already exists." } }
            }, ct);
            return;
        }

        var team = new Team(req.Name);
        await _teamRepository.CreateAsync(team);

        var response = new TeamResponse
        {
            Id = team.Id,
            Name = team.Name,
            CreatedAt = team.CreatedAt
        };

        HttpContext.Response.StatusCode = StatusCodes.Status201Created;
        await HttpContext.Response.WriteAsJsonAsync(response, ct);
    }
}
