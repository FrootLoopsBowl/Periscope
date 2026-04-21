using Domain.Constants.User;
using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Web.Features.Admins.Teams.Events.UpdateTeamEvent;

namespace Tests.Web.Features.Admins.Teams.Events.UpdateTeamEvent;

public class UpdateTeamEventEndpointTests
{
    private readonly Mock<ITeamEventRepository> _teamEventRepository;
    private readonly UpdateTeamEventEndpoint _endpoint;

    public UpdateTeamEventEndpointTests()
    {
        _teamEventRepository = new Mock<ITeamEventRepository>();
        _endpoint = Factory.Create<UpdateTeamEventEndpoint>(_teamEventRepository.Object);
    }

    [Fact]
    public void WhenConfigure_ThenConfigureVerbToBePut()
    {
        _endpoint.Configure();
        _endpoint.Definition.Verbs.ShouldContain(Http.PUT.ToString());
    }

    [Fact]
    public void WhenConfigure_ThenConfigureRoute()
    {
        _endpoint.Configure();
        _endpoint.Definition.Routes.ShouldContain("teams/{TeamId}/events/{EventId}");
    }

    [Fact]
    public void WhenConfigure_ThenConfigureAllowedRoles()
    {
        _endpoint.Configure();
        _endpoint.Definition.AllowedRoles!.ShouldContain(Roles.ADMINISTRATOR);
    }

    [Fact]
    public void WhenConfigure_ThenConfigureAuthSchemeToBeJwtBearer()
    {
        _endpoint.Configure();
        _endpoint.Definition.AuthSchemeNames!.ShouldContain(JwtBearerDefaults.AuthenticationScheme);
    }

    [Fact]
    public async Task WhenHandleAsync_AndEventNotFound_ThenReturn404()
    {
        _teamEventRepository.Setup(x => x.FindByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TeamEvent?)null);
        var request = new UpdateTeamEventRequest
        {
            TeamId = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            Type = "Match",
            StartDateTime = new DateTime(2026, 4, 9, 18, 0, 0, DateTimeKind.Utc),
            EndDateTime = new DateTime(2026, 4, 9, 19, 30, 0, DateTimeKind.Utc)
        };

        await _endpoint.HandleAsync(request, CancellationToken.None);

        _endpoint.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task WhenHandleAsync_AndEventBelongsToDifferentTeam_ThenReturn404()
    {
        var teamId = Guid.NewGuid();
        var differentTeamId = Guid.NewGuid();
        var teamEvent = new TeamEvent(differentTeamId, EventType.Pratique,
            new DateTime(2026, 4, 9, 18, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 9, 19, 30, 0, DateTimeKind.Utc));
        teamEvent.SetId(Guid.NewGuid());
        _teamEventRepository.Setup(x => x.FindByIdAsync(teamEvent.Id)).ReturnsAsync(teamEvent);

        var request = new UpdateTeamEventRequest
        {
            TeamId = teamId,
            EventId = teamEvent.Id,
            Type = "Match",
            StartDateTime = new DateTime(2026, 4, 10, 14, 0, 0, DateTimeKind.Utc),
            EndDateTime = new DateTime(2026, 4, 10, 16, 0, 0, DateTimeKind.Utc)
        };

        await _endpoint.HandleAsync(request, CancellationToken.None);

        _endpoint.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task WhenHandleAsync_AndEventFound_ThenReturn204()
    {
        var teamId = Guid.NewGuid();
        var teamEvent = new TeamEvent(teamId, EventType.Pratique,
            new DateTime(2026, 4, 9, 18, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 9, 19, 30, 0, DateTimeKind.Utc));
        teamEvent.SetId(Guid.NewGuid());
        _teamEventRepository.Setup(x => x.FindByIdAsync(teamEvent.Id)).ReturnsAsync(teamEvent);

        var request = new UpdateTeamEventRequest
        {
            TeamId = teamId,
            EventId = teamEvent.Id,
            Type = "Match",
            StartDateTime = new DateTime(2026, 4, 10, 14, 0, 0, DateTimeKind.Utc),
            EndDateTime = new DateTime(2026, 4, 10, 16, 0, 0, DateTimeKind.Utc)
        };

        await _endpoint.HandleAsync(request, CancellationToken.None);

        _endpoint.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status204NoContent);
    }
}
