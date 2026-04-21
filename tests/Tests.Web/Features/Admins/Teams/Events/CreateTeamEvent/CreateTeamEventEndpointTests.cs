using Domain.Constants.User;
using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Web.Features.Admins.Teams.Events.CreateTeamEvent;

namespace Tests.Web.Features.Admins.Teams.Events.CreateTeamEvent;

public class CreateTeamEventEndpointTests
{
    private readonly Mock<ITeamEventRepository> _teamEventRepository;
    private readonly Mock<ITeamRepository> _teamRepository;
    private readonly CreateTeamEventEndpoint _endpoint;

    public CreateTeamEventEndpointTests()
    {
        _teamEventRepository = new Mock<ITeamEventRepository>();
        _teamRepository = new Mock<ITeamRepository>();
        _endpoint = Factory.Create<CreateTeamEventEndpoint>(
            _teamEventRepository.Object,
            _teamRepository.Object
        );
    }

    [Fact]
    public void WhenConfigure_ThenConfigureVerbToBePost()
    {
        _endpoint.Configure();
        _endpoint.Definition.Verbs.ShouldContain(Http.POST.ToString());
    }

    [Fact]
    public void WhenConfigure_ThenConfigureRoute()
    {
        _endpoint.Configure();
        _endpoint.Definition.Routes.ShouldContain("teams/{TeamId}/events");
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
    public async Task WhenHandleAsync_AndTeamNotFound_ThenReturn404()
    {
        _teamRepository.Setup(x => x.FindByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Team?)null);
        var request = new CreateTeamEventRequest
        {
            TeamId = Guid.NewGuid(),
            Type = "Pratique",
            StartDateTime = new DateTime(2026, 4, 9, 18, 0, 0, DateTimeKind.Utc),
            EndDateTime = new DateTime(2026, 4, 9, 19, 30, 0, DateTimeKind.Utc)
        };

        await _endpoint.HandleAsync(request, CancellationToken.None);

        _endpoint.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task WhenHandleAsync_AndTeamFound_ThenReturn201()
    {
        var team = new Team("Équipe A");
        team.SetId(Guid.NewGuid());
        _teamRepository.Setup(x => x.FindByIdAsync(team.Id)).ReturnsAsync(team);
        var request = new CreateTeamEventRequest
        {
            TeamId = team.Id,
            Type = "Pratique",
            StartDateTime = new DateTime(2026, 4, 9, 18, 0, 0, DateTimeKind.Utc),
            EndDateTime = new DateTime(2026, 4, 9, 19, 30, 0, DateTimeKind.Utc)
        };

        await _endpoint.HandleAsync(request, CancellationToken.None);

        _endpoint.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task WhenHandleAsync_AndTeamFound_ThenDelegateToRepository()
    {
        var team = new Team("Équipe A");
        team.SetId(Guid.NewGuid());
        _teamRepository.Setup(x => x.FindByIdAsync(team.Id)).ReturnsAsync(team);
        var request = new CreateTeamEventRequest
        {
            TeamId = team.Id,
            Type = "Pratique",
            StartDateTime = new DateTime(2026, 4, 9, 18, 0, 0, DateTimeKind.Utc),
            EndDateTime = new DateTime(2026, 4, 9, 19, 30, 0, DateTimeKind.Utc)
        };

        await _endpoint.HandleAsync(request, CancellationToken.None);

        _teamEventRepository.Verify(x => x.CreateAsync(It.Is<TeamEvent>(e =>
            e.TeamId == team.Id &&
            e.Type == EventType.Pratique)));
    }
}
