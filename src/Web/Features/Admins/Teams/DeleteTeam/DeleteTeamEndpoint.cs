using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Teams.DeleteTeam;

public class DeleteTeamEndpoint : Endpoint<DeleteTeamRequest, EmptyResponse>
{
    private readonly ITeamRepository _teamRepository;

    public DeleteTeamEndpoint(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
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
        var team = await _teamRepository.FindByIdAsync(req.Id);
        if (team is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await _teamRepository.DeleteAsync(team);
        await Send.NoContentAsync(ct);
    }
}
