using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Teams.Events.GetTeamEvents;

public class GetTeamEventsEndpoint : Endpoint<GetTeamEventsRequest, object>
{
    private readonly ITeamEventRepository _teamEventRepository;
    private readonly ITeamRepository _teamRepository;

    public GetTeamEventsEndpoint(ITeamEventRepository teamEventRepository, ITeamRepository teamRepository)
    {
        _teamEventRepository = teamEventRepository;
        _teamRepository = teamRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("teams/{TeamId}/events");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetTeamEventsRequest req, CancellationToken ct)
    {
        var team = await _teamRepository.FindByIdAsync(req.TeamId);
        if (team is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var from = req.From ?? DateTime.UtcNow.AddMonths(-1);
        var to = req.To ?? DateTime.UtcNow.AddMonths(1);

        var events = await _teamEventRepository.GetByTeamIdAndRangeAsync(req.TeamId, from, to);

        var response = events.Select(e => new TeamEventResponse
        {
            Id = e.Id,
            Type = e.Type.ToString(),
            StartDateTime = e.StartDateTime,
            EndDateTime = e.EndDateTime
        });

        await Send.OkAsync(response, cancellation: ct);
    }
}
