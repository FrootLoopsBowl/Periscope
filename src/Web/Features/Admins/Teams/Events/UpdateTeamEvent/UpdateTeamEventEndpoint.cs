using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Teams.Events.UpdateTeamEvent;

public class UpdateTeamEventEndpoint : Endpoint<UpdateTeamEventRequest, EmptyResponse>
{
    private readonly ITeamEventRepository _teamEventRepository;

    public UpdateTeamEventEndpoint(ITeamEventRepository teamEventRepository)
    {
        _teamEventRepository = teamEventRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Put("teams/{TeamId}/events/{EventId}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateTeamEventRequest req, CancellationToken ct)
    {
        var teamEvent = await _teamEventRepository.FindByIdAsync(req.EventId);
        if (teamEvent is null || teamEvent.TeamId != req.TeamId)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var type = Enum.Parse<EventType>(req.Type);
        teamEvent.Update(type, req.StartDateTime, req.EndDateTime);
        await _teamEventRepository.UpdateAsync(teamEvent);

        await Send.NoContentAsync(ct);
    }
}
