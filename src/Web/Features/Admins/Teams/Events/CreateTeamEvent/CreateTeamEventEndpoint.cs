using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Features.Admins.Teams.Events.GetTeamEvents;

namespace Web.Features.Admins.Teams.Events.CreateTeamEvent;

public class CreateTeamEventEndpoint : Endpoint<CreateTeamEventRequest, TeamEventResponse>
{
    private readonly ITeamEventRepository _teamEventRepository;
    private readonly ITeamRepository _teamRepository;

    public CreateTeamEventEndpoint(ITeamEventRepository teamEventRepository, ITeamRepository teamRepository)
    {
        _teamEventRepository = teamEventRepository;
        _teamRepository = teamRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("teams/{TeamId}/events");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateTeamEventRequest req, CancellationToken ct)
    {
        var team = await _teamRepository.FindByIdAsync(req.TeamId);
        if (team is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var type = Enum.Parse<EventType>(req.Type);
        var teamEvent = new TeamEvent(req.TeamId, type, req.StartDateTime, req.EndDateTime, req.Description);
        await _teamEventRepository.CreateAsync(teamEvent);

        var response = new TeamEventResponse
        {
            Id = teamEvent.Id,
            Type = teamEvent.Type.ToString(),
            StartDateTime = teamEvent.StartDateTime,
            EndDateTime = teamEvent.EndDateTime,
            Description = teamEvent.Description
        };

        HttpContext.Response.StatusCode = StatusCodes.Status201Created;
        await HttpContext.Response.WriteAsJsonAsync(response, ct);
    }
}
