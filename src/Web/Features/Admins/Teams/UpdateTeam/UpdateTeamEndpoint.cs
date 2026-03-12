using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Teams.UpdateTeam;

public class UpdateTeamEndpoint : Endpoint<UpdateTeamRequest, EmptyResponse>
{
    private readonly ITeamRepository _teamRepository;

    public UpdateTeamEndpoint(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Put("teams/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateTeamRequest req, CancellationToken ct)
    {
        var team = await _teamRepository.FindByIdAsync(req.Id);
        if (team is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (team.Name != req.Name)
        {
            var nameAlreadyUsed = await _teamRepository.ExistsByNameAsync(req.Name);
            if (nameAlreadyUsed)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await HttpContext.Response.WriteAsJsonAsync(new
                {
                    errors = new[] { new { errorType = "TeamWithNameAlreadyExists", errorMessage = "A team with this name already exists." } }
                }, ct);
                return;
            }
        }

        team.SetName(req.Name);
        await _teamRepository.UpdateAsync(team);
        await Send.NoContentAsync(ct);
    }
}