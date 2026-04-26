using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Teams.Events.DeleteTeamEvent;

public class DeleteTeamEventEndpoint : Endpoint<DeleteTeamEventRequest, EmptyResponse>
{
    private readonly ITeamEventRepository _teamEventRepository;

    public DeleteTeamEventEndpoint(ITeamEventRepository teamEventRepository)
    {
        _teamEventRepository = teamEventRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Delete("teams/{TeamId}/events/{EventId}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeleteTeamEventRequest req, CancellationToken ct)
    {
        var teamEvent = await _teamEventRepository.FindByIdAsync(req.EventId);
        if (teamEvent is null || teamEvent.TeamId != req.TeamId)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await _teamEventRepository.DeleteAsync(teamEvent);
        await Send.NoContentAsync(ct);
    }
}
